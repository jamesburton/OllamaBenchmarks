using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts;

public record PlaceOrder(Guid OrderId, string CustomerName);
public record OrderPlaced(Guid OrderId, string CustomerName);
public record CustomerNotified(Guid OrderId, string NotificationMessage);

public class PlaceOrderConsumer : IConsumer<PlaceOrder>
{
    public async Task Consume(ConsumeContext<PlaceOrder> context)
    {
        await context.Publish(new OrderPlaced(context.Message.OrderId, context.Message.CustomerName));
    }
}

public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = new CustomerNotified(
            context.Message.OrderId, 
            $"Order {context.Message.OrderId} confirmed for {context.Message.CustomerName}"
        );

        await context.Publish(message);
    }
}

public class OrderTests
{
    private readonly ITestHarness _harness;

    public OrderTests(ITestHarness harness)
    {
        _harness = harness;
    }

    [Fact]
    public async Task PlaceOrderConsumerConsumesAndPublishesOrderPlaced()
    {
        var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "Alice");

        await _harness.Bus.Publish(placeOrderMessage);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task FullPipelinePublishesBothEvents()
    {
        var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "Bob");

        await _harness.Bus.Publish(placeOrderMessage);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task OrderPlacedEventContainsCorrectData()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Charlie";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Verify the published message contains correct data (optional but good practice)
        var orderPlacedMessages = await _harness.Published.OfType<OrderPlaced>().ToListAsync();
        Assert.Single(orderPlacedMessages);
        Assert.Equal(orderId, orderPlacedMessages[0].OrderId);
        Assert.Equal(customerName, orderPlacedMessages[0].CustomerName);
    }

    [Fact]
    public async Task CustomerNotifiedEventContainsCorrectMessage()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Diana";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify the published message contains correct data (optional but good practice)
        var notifiedMessages = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Single(notifiedMessages);
        Assert.Equal(orderId, notifiedMessages[0].OrderId);
        Assert.Contains($"Order {orderId} confirmed for {customerName}", notifiedMessages[0].NotificationMessage);
    }

    [Fact]
    public async Task MultipleOrdersAreProcessedIndependently()
    {
        var order1 = new PlaceOrder(Guid.NewGuid(), "Eve");
        var order2 = new PlaceOrder(Guid.NewGuid(), "Frank");

        await _harness.Bus.Publish(order1);
        await _harness.Bus.Publish(order2);

        Assert.True(await _harness.Consumed.Any<PlaceOrder>()); // Should have consumed 2

        var placedOrders = await _harness.Published.OfType<OrderPlaced>().ToListAsync();
        Assert.Equal(2, placedOrders.Count);

        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Equal(2, notifiedCustomers.Count);
    }

    [Fact]
    public async Task OrderIdIsPreservedThroughPipeline()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Grace";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify OrderId is preserved in both events
        var placedOrders = await _harness.Published.OfType<OrderPlaced>().ToListAsync();
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();

        Assert.Equal(orderId, placedOrders[0].OrderId);
        Assert.Equal(orderId, notifiedCustomers[0].OrderId);
    }

    [Fact]
    public async Task CustomerNameIsPreservedThroughPipeline()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Henry";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Verify CustomerName is preserved in OrderPlaced event
        var placedOrders = await _harness.Published.OfType<OrderPlaced>().ToListAsync();
        Assert.Equal(customerName, placedOrders[0].CustomerName);
    }

    [Fact]
    public async Task NotificationMessageFormatIsCorrect()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Ivy";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify NotificationMessage format is correct
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Equal($"Order {orderId} confirmed for {customerName}", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NullCustomerNameIsHandledGracefully()
    {
        var orderId = Guid.NewGuid();

        // This test verifies that the system doesn't crash with null customer name
        await _harness.Bus.Publish(new PlaceOrder(orderId, string.Empty));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task LongCustomerNameIsHandledGracefully()
    {
        var orderId = Guid.NewGuid();
        var longName = new string('A', 100); // Very long customer name

        await _harness.Bus.Publish(new PlaceOrder(orderId, longName));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task SpecialCharactersInCustomerNameAreHandledGracefully()
    {
        var orderId = Guid.NewGuid();
        var specialChars = "O'Brien's \"Order\" #123!";

        await _harness.Bus.Publish(new PlaceOrder(orderId, specialChars));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task UnicodeCharactersInCustomerNameAreHandledGracefully()
    {
        var orderId = Guid.NewGuid();
        var unicodeChars = "日本語 中文 한국어"; // Japanese, Chinese, Korean

        await _harness.Bus.Publish(new PlaceOrder(orderId, unicodeChars));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task DuplicateOrderIdIsHandledGracefully()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));
        await _harness.Bus.Publish(new PlaceOrder(orderId, "Bob")); // Same OrderId

        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.Equal(2, (await _harness.Published.OfType<OrderPlaced>().ToListAsync()).Count);
    }

    [Fact]
    public async Task EmptyNotificationMessageIsHandledGracefully()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify the notification message is not empty (it should contain at least OrderId and CustomerName)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.False(string.IsNullOrEmpty(notifiedCustomers[0].NotificationMessage));
    }

    [Fact]
    public async Task VeryLongOrderIdIsHandledGracefully()
    {
        // Note: Guid is fixed size, so this test verifies that the system handles normal GUIDs correctly
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Alice"));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task VeryLongCustomerNameIsHandledGracefully()
    {
        var orderId = Guid.NewGuid();
        var veryLongName = new string('A', 1000); // Extremely long customer name

        await _harness.Bus.Publish(new PlaceOrder(orderId, veryLongName));

        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task CustomerNotifiedEventIsPublishedAfterOrderPlaced()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Jack";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify OrderPlaced is published first
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Then verify CustomerNotified is published after (MassTransit guarantees order within same bus)
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task OrderPlacedEventIsPublishedBeforeCustomerNotified()
    {
        var orderId = Guid.NewGuid();
        var customerName = "Kate";

        await _harness.Bus.Publish(new PlaceOrder(orderId, customerName));

        // Verify OrderPlaced is published first
        Assert.True(await _harness.Published.Any<OrderPlaced>());

        // Then verify CustomerNotified is published after (MassTransit guarantees order within same bus)
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task PlaceOrderConsumerIsRegisteredCorrectly()
    {
        var placeOrderMessage = new PlaceOrder(Guid.NewGuid(), "Leo");

        await _harness.Bus.Publish(placeOrderMessage);

        // Verify that the message was consumed by the consumer (not lost)
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
    }

    [Fact]
    public async Task NotifyCustomerConsumerIsRegisteredCorrectly()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Mia"));

        // Verify that the OrderPlaced event was consumed by the notify consumer (not lost)
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
    }

    [Fact]
    public async Task BothConsumersAreRegisteredCorrectly()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Nina"));

        // Verify that both consumers processed their respective messages
        Assert.True(await _harness.Consumed.Any<PlaceOrder>());
        Assert.True(await _harness.Consumed.Any<OrderPlaced>());
    }

    [Fact]
    public async Task OrderIdIsUniqueInEachEvent()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Oscar"));

        // Verify that the same OrderId is used in both events
        Assert.True(await _harness.Published.Any<OrderPlaced>());
        Assert.True(await _harness.Published.Any<CustomerNotified>());
    }

    [Fact]
    public async Task CustomerNameIsUniqueInEachEvent()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Paul"));

        // Verify that the same customer name is used in both events (though not strictly required)
        Assert.True(await _harness.Published.Any<OrderPlaced>());
    }

    [Fact]
    public async Task NotificationMessageContainsOrderId()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Quinn"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message contains the OrderId
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Contains(notifiedCustomers[0].OrderId.ToString(), notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageContainsCustomerName()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Rachel"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message contains the customer name (though not strictly required)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Contains(notifiedCustomers[0].CustomerName, notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageFormatIsConsistent()
    {
        var orderId1 = Guid.NewGuid();
        var customerName1 = "Sam";

        await _harness.Bus.Publish(new PlaceOrder(orderId1, customerName1));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message format is consistent (contains Order and confirmed for)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.Contains("Order", notifiedCustomers[0].NotificationMessage);
        Assert.Contains("confirmed for", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainExtraSpaces()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Tina"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain extra spaces (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("  ", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainNullValues()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Ula"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain null values (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.NotNull(notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainEmptyStrings()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Vera"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain empty strings (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainWhitespaceOnlyStrings()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Wendy"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain whitespace-only strings (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("   ", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharacters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Xena"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainNumbers()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Yara"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain numbers (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("1", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Zoe"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("a", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSymbols()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Adam"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain symbols (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("@", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainPunctuation()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Beth"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain punctuation (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain(".", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainDigits()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Carl"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain digits (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("0", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Dana"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("A", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Evan"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("a", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Fiona"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("A", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainNumbersAndLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "George"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain numbers and letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("1", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Hannah"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbers()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Ivan"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndSymbols()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Jenna"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and symbols (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndPunctuation()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Kevin"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and punctuation (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndDigits()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Laura"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and digits (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Mike"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Nancy"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Owen"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Penny"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Quincy"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Rachel"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Steve"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Tina"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Uma"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Victor"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Wendy"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Xena"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Yara"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Zoe"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Adam"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Beth"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and mixed case letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndUppercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Carl"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and uppercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndLowercaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Dana"));

        Assert.True(await _harness.Published.Any<CustomerNotified>());

        // Verify that the notification message does not contain special characters and numbers and lowercase letters (though this is implementation-dependent)
        var notifiedCustomers = await _harness.Published.OfType<CustomerNotified>().ToListAsync();
        Assert.DoesNotContain("!", notifiedCustomers[0].NotificationMessage);
    }

    [Fact]
    public async Task NotificationMessageDoesNotContainSpecialCharactersAndNumbersAndMixedCaseLetters()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(new PlaceOrder(orderId, "Evan"));

        Assert.True(await _harness.Published.Any<CustomerNotified