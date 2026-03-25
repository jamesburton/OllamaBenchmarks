using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Xunit;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Saga.StateManagement;
using MassTransit.Testing;

namespace OrderFulfillmentSaga
{
    public class OrderFulfillmentSagaTests : IAsyncLifetime
    {
        private readonly Mock<IBus> _busMock;
        private readonly Mock<ISagaRepository<OrderFulfillmentState>> _sagaRepositoryMock;
        private readonly OrderFulfillmentState _state;

        [Fact]
        public async Task Test()
        {
            // Arrange

            var fixture = new DatabaseFixture();
            var serviceCollection = fixture.ServiceProvider;

            var busConfigurator = serviceCollection.GetRequiredService<IBusConfigurator>();
            var sagaConfigurator = serviceCollection.GetRequiredService<ISagaConfigurator>();

            var harness = await serviceCollection.GetRequiredService<ITestHarness>()
                .StartAsync();

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer<OrderCompleted>();
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer<OrderShipped>();
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced>();
            busConfig.AddConsumer<PaymentReceived>();
            busConfig.AddConsumer(OrderShipped);
            busConfig.AddConsumer(OrderCompleted);
            var sagaStateConfig = new OrderFulfillmentSagaStateConfig();
            sagaStateConfig.InMemoryRepository = true;
            HarnessBusConfigurator.SagaStateConfigurations.Add(sagaStateConfig);

            // Register the Saga State Machine with in-memory repository
            HarnessBusConfigurator busConfig = new HarnessBusConfigurator();
            busConfig.AddConsumer<OrderPlaced());