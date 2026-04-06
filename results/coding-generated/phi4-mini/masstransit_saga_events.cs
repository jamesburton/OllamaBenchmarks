global using System;
global using System.Collections.Generic;
global using System.Linq;
global using MassTransit;
namespace Contracts
{
    public class InvoiceSaga : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public decimal Amount { get; private set; }

        [Serializable]
        record InvoiceCreated(Guid InvoiceId, decimal Amount);

        [Serializable]
        record InvoiceApproved(Guid InvoiceId);

        [Serializable]
        record InvoicePaid(Guid InvoiceId);
    }

    public class OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public decimal Amount { get; private set; }

        public OrderPlaced() {}

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            => (CorrelationId, CurrentState, Amount) = (correlationId, currentState, amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; }

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId)
            , CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState, amount))
            { Amount = amount; }
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId)
            , CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentState);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId),
                CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentState),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId),
                CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentState),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId),
                CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentState),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId),
                CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentState),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceId),
                CurrentState(currentState),
                CorrelatedOrder(new OrderPlaced(correlationId, currentState)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceApproved(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentState);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoicePaid(Guid invoiceId, string currentState)
            : this(invoiceId),
                CurrentState(currentstate);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (CorrelationId, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationId, string currentState, decimal amount)
            : this(correlationId, currentstate),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceId) => (InvoiceId, ) = invoiceId;

        public InvoiceCreated(Guid invoiceId, string currentState, decimal amount)
            : this(invoiceid),
                Currentstate(currentstate),
                CorrelatedOrder(new OrderPlaced(correlationid, currentstate)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceId) => (InvoiceId, ) = invoiceid;

        public InvoiceApproved(Guid invoiceId, string currentstate)
            : this(invoiceid),
                Currentstate(currentstate);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceId) => (InvoiceId, ) = invoiceid;

        public InvoicePaid(Guid invoiceId, currentstate)
            : this(invoiceid),
                Currentstate(currentstate);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (Correlationid, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationid, currentstate, amount)
            : this(correlationid, currentstate),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceCreated(Guid invoiceid, string currentstate, amount)
            : this(invoiceid),
                CurrentState(currentstate),
                CorrelatedOrder(new OrderPlaced(correlationid, currentstate)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceApproved(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoicePaid(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (Correlationid, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationid, currentstate, amount)
            : this(correlationid, currentstate),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceCreated(Guid invoiceid, string currentstate, amount)
            : this(invoiceid),
                CurrentState(currentstate),
                CorrelatedOrder(new OrderPlaced(correlationid, currentstate)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceApproved(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoicePaid(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (Correlationid, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationid, currentstate, amount)
            : this(correlationid, currentstate),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get; init;}

        public InvoiceCreated(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceCreated(Guid invoiceid, string currentstate, amount)
            : this(invoiceid),
                CurrentState(currentstate),
                CorrelatedOrder(new OrderPlaced(correlationid, currentstate)),
                Amount(amount);
    }

    [Serializable]
    record InvoiceApproved : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoiceApproved(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoiceApproved(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record InvoicePaid : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public string CurrentState { get; private set; } 

        public InvoicePaid(Guid invoiceid) => (InvoiceId, ) = invoiceid;

        public InvoicePaid(Guid invoiceid, currentstate)
            : this(invoiceid),
                CurrentState(currentstate);
    }

    [Serializable]
    record OrderPlaced : IEvent
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; private set; } 

        public decimal Amount { get; init;}

        public OrderPlaced() => (Correlationid, ) = Guid.NewGuid();

        public OrderPlaced(Guid correlationid, currentstate, amount)
            : this(correlationid, currentstate),
                Amount(amount);
    }

    [Serializable]
    record InvoiceCreated : IEvent
    {
        public Guid InvoiceId { get; set; } 

        public OrderPlaced CorrelatedOrder { get; private set; } 

        public decimal Amount { get;