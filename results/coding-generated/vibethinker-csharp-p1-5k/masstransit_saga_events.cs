global using Contracts;
using MassTransit;
namespace Contracts;

public class InvoiceSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; private set; }
    public decimal Amount { get; set; }

    public Event<InvoiceCreated> InvoiceCreated { get; private set; } = null!;
    public Event<InvoiceApproved> InvoiceApproved { get; private set; } = null!;
    public Event<InvoicePaid> InvoicePaid { get; private set; } = null!;

    public InvoiceStateMachine InvoiceStateMachine : MassTransitStateMachine<InvoiceSaga>
    {
        public State Pending { get; private set; }
        public State Approved { get; private set; }
        public State Paid { get; private set; }

        public Event<InvoiceCreated> InvoiceCreated { get; private set; } = null!;
        public Event<InvoiceApproved> InvoiceApproved { get; private set; } = null!;
        public Event<InvoicePaid> InvoicePaid { get; private set; } = null!;

        public InvoiceStateMachine()
        {
            Initially(
                When(InvoiceCreated)
                    .TransitionTo(Pending));

            During(Pending,
                When(InvoiceApproved)
                    .TransitionTo(Approved));

            During(Approved,
                When(InvoicePaid)
                    .TransitionTo(Paid).Finalize());

            SetCompletedWhenFinalized();
        }
    }

    public void OnMessage(string message, string correlationId, decimal amount) => InvoiceSaga.CurrentState = CorrelationId;
    public void OnMessage(string message, string correlationId, decimal amount, string state) => CurrentState = state;

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, decimal amount, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<InvoiceApproved>(message, correlationId, amount).Invoke()
            , Event<InvoicePaid>(message, correlationId, amount).Invoke();

    public void OnMessage(string message, string correlationId, string state, string event) => Event<InvoiceCreated>(message, correlationId, amount).Invoke();
            , Event<Invoice