using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineSystem
{
    /// <summary>
    /// Represents a multi-stage asynchronous pipeline.
    /// </summary>
    /// <typeparam name="TIn">The input type of the pipeline.</typeparam>
    /// <typeparam name="TOut">The output type of the pipeline.</typeparam>
    public class AsyncPipeline<TIn, TOut>
    {
        private readonly List<Func<object, CancellationToken, Task<object>>> _stages;

        /// <summary>
        /// Initializes a new instance of the AsyncPipeline class.
        /// </summary>
        /// <param name="stages">A collection of stage functions that take an object and CancellationToken and return a Task of object.</param>
        public AsyncPipeline(IEnumerable<Func<object, CancellationToken, Task<object>>> stages)
        {
            _stages = new List<Func<object, CancellationToken, Task<object>>>(stages);
        }

        /// <summary>
        /// Executes the pipeline, passing the input through all stages in sequence.
        /// </summary>
        /// <param name="input">The input to the pipeline.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The result of the pipeline execution.</returns>
        public async Task<TOut> ExecuteAsync(TIn input, CancellationToken ct)
        {
            object current = input!;

            foreach (var stage in _stages)
            {
                ct.ThrowIfCancellationRequested();
                current = await stage(current, ct).ConfigureAwait(false);
            }

            return (TOut)current;
        }

        /// <summary>
        /// Factory method to build a pipeline from loosely typed stages.
        /// </summary>
        /// <param name="stages">An array of stage delegates.</param>
        /// <returns>A new AsyncPipeline instance.</returns>
        public static AsyncPipeline<TIn, TOut> Build<TIn, TOut>(params object[] stages)
        {
            var compiledStages = new List<Func<object, CancellationToken, Task<object>>>();

            foreach (var stage in stages)
            {
                if (stage == null) throw new ArgumentNullException(nameof(stages), "Stage cannot be null.");

                var stageType = stage.GetType();
                if (!typeof(Delegate).IsAssignableFrom(stageType))
                {
                    throw new ArgumentException($"Stage {stage} is not a delegate.", nameof(stages));
                }

                // We wrap the specific delegate into the common (object, CT) -> Task<object> shape
                compiledStages.Add(WrapStage(stage));
            }

            return new AsyncPipeline<TIn, TOut>(compiledStages);
        }

        private static Func<object, CancellationToken, Task<object>> WrapStage(object stage)
        {
            var stageType = stage.GetType();
            var method = stageType.GetMethod("Invoke");
            var parameters = method!.GetParameters();

            // Validate signature: (T, CancellationToken) -> Task<TNext>
            if (parameters.Length != 2 || parameters[1].ParameterType != typeof(CancellationToken))
            {
                throw new ArgumentException($"Stage {stage} must have signature Func<T, CancellationToken, Task<TNext>.");
            }

            var inputParam = Expression.Parameter(typeof(object), "input");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            // Cast input object to the specific input type of the delegate
            var castInput = Expression.Convert(inputParam, parameters[0].ParameterType);

            // Call the delegate
            var call = Expression.Call(Expression.Constant(stage), method, castInput, ctParam);

            // The result is a Task<TNext>. We need to convert it to Task<object>.
            // We do this by awaiting the task and boxing the result.
            // Expression: await (Task<TNext>)call
            // Expression: (object)result

            var taskType = method.ReturnType;
            if (!typeof(Task).IsAssignableFrom(taskType))
            {
                throw new ArgumentException($"Stage {stage} must return a Task.");
            }

            // We build a lambda that performs: async (input, ct) => (object)await ((Func<...>)stage)((T)input, ct)
            var wrapperLambda = Expression.Lambda<Func<object, CancellationToken, Task<object>>>(
                ConvertTaskToObject(call, taskType),
                inputParam,
                ctParam
            );

            return wrapperLambda.Compile();
        }

        private static Expression ConvertTaskToObject(Expression taskExpression, Type taskType)
        {
            // We want to generate: async (input, ct) => { var result = await task; return (object)result; }
            // However, since we are building expressions, we can use the TaskContinuation approach or 
            // simply rely on dynamic compilation of an async lambda.

            // Helper method to invoke generic logic
            var converterMethod = typeof(AsyncPipeline<TIn, TOut>).GetMethod(nameof(WrapGenericAsync), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            // Get the TNext type from Task<TNext>
            var resultType = taskType.IsGenericType ? taskType.GetGenericArguments()[0] : typeof(void);

            if (resultType == typeof(void))
            {
                throw new ArgumentException("Stages returning Task (void) are not supported in this implementation; must return Task<T>.");
            }

            // Call generic helper: WrapGenericAsync<TNext>(task)
            var genericMethod = converterMethod!.MakeGenericMethod(resultType);

            // Expression: WrapGenericAsync<TNext>((Task<TNext>)taskExpression)
            var callWrap = Expression.Call(genericMethod, taskExpression);

            return callWrap;
        }

        private static async Task<object> WrapGenericAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Provides a fluent API for building AsyncPipeline instances.
    /// </summary>
    public static class PipelineBuilder
    {
        public static PipelineBuilder<TIn, TOut> Start<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> first)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            return new PipelineBuilder<TIn, TOut>(first);
        }
    }

    /// <summary>
    /// Fluent builder for constructing a pipeline step-by-step.
    /// </summary>
    public class PipelineBuilder<TIn, TOut>
    {
        private readonly List<Func<object, CancellationToken, Task<object>>> _stages;

        internal PipelineBuilder(Func<TIn, CancellationToken, Task<TOut>> first)
        {
            _stages = new List<Func<object, CancellationToken, Task<object>>>
            {
                // Wrap the strongly typed first stage into the common weakly typed storage
                async (input, ct) => 
                {
                    var result = await first((TIn)input, ct).ConfigureAwait(false);
                    return result!;
                }
            };
        }

        private PipelineBuilder(List<Func<object, CancellationToken, Task<object>>> stages)
        {
            _stages = stages;
        }

        public PipelineBuilder<TIn, TNext> Then<TNext>(Func<TOut, CancellationToken, Task<TNext>> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            // Create a new list of stages to ensure immutability of the previous builder
            var newStages = new List<Func<object, CancellationToken, Task<object>>>(_stages);

            newStages.Add(async (input, ct) =>
            {
                var result = await next((TOut)input, ct).ConfigureAwait(false);
                return result!;
            });

            return new PipelineBuilder<TIn, TNext>(newStages);
        }

        public AsyncPipeline<TIn, TOut> Build()
        {
            return new AsyncPipeline<TIn, TOut>(_stages);
        }
    }
}