using Contracts;
using MassTransit;
using System.Threading.Tasks;

namespace Contracts
{
    public class InvoiceSaga : SagaStateMachine<InvoiceSaga>
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public decimal Amount { get; set; }

        protected override void ConfigureSaga()
        {
            Instance(() => CorrelationId);
            Instance(() => CurrentState);
            Instance(() => Amount);

            RegisterSagaEvent<InvoiceCreated>();
            RegisterSagaEvent<InvoiceApproved>();
            RegisterSagaEvent<InvoicePaid>();
        }

        public InvoiceSaga()
        {
            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, () => null);
            Instance(() => Amount, () => 0m);

            RegisterSagaEvent<InvoiceCreated>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoiceApproved>(x => x.CorrelateById(m => m.Message.InvoiceId));
            RegisterSagaEvent<InvoicePaid>(x => x.CorrelateById(m => m.Message.InvoiceId));

            Instance(() => CurrentState, ()