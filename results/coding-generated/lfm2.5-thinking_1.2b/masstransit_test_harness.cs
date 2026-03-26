public class OrderProcessingTests
{
    [Test]
    public void VerifyConsumerProcessing()
    {
        Assert.Multiple(
            Assert.Equal("Order123", expectedOrderId),
            Assert.Equal("PaymentProcessed", expectedEvent);
        );
    }
}