using System;
using System.Threading.Tasks;

namespace Contracts
{
    public class SubmitOrder
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
    }

    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);
}