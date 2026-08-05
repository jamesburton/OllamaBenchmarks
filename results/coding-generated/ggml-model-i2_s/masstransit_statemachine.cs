using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;

namespace MassTransit
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? AcceptedAt { get; set; }

        public OrderState()
        {
            Initialize();
        }

        public void Initialize()
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void TransitionTo(OrderState state)
        {
            CorrelationId = state.CorrelationId;
            CurrentState = state.CurrentState;
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTime.Now;
        }

        public void TransitionTo(Processing)
        {
            CorrelationId = Guid.NewGuid();
            CurrentState = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderDate = DateTime.Now;
            AcceptedAt = DateTimeNow