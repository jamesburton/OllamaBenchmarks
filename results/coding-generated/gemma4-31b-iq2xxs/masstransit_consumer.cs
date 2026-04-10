public async Task Consume(ConsumeContext<SubmitOrder> context) {
            var msg = context.Message;
            await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
        }