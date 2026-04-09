var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        Assert.Multiple(() => Assert.True(consumed), () => Assert.True(published));

var consumed = await harness.Consumed.Any<ProcessPayment>();
        var published = await harness.Published.Any<PaymentProcessed>();
        Assert.Multiple(() => Assert.True(consumed), () => Assert.True(published));