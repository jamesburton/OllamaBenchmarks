var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>(p => p.OrderId == orderId);
        Assert.Multiple(() =>
        {
            Assert.True(consumed);
            Assert.True(published);
        });