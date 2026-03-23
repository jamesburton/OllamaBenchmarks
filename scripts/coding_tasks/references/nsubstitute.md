# NSubstitute v5.3.0 — Quick Reference

**Namespace:** `using NSubstitute;`

## Create Substitutes

```csharp
var service = Substitute.For<IOrderService>();
var multi = Substitute.For<IFoo, IBar>(); // multiple interfaces
```

## Configure Return Values

```csharp
service.GetOrder(1).Returns(new Order { Id = 1 });
service.GetOrder(Arg.Any<int>()).Returns(new Order());
service.GetOrder(Arg.Is<int>(id => id > 0)).Returns(new Order());

// Dynamic return
service.GetOrder(1).Returns(callInfo => new Order { Id = callInfo.Arg<int>() });

// Return for any args
service.GetOrder(0).ReturnsForAnyArgs(new Order());
```

## Async — auto-wraps in Task/ValueTask

```csharp
service.GetAsync(1).Returns(new Item { Id = 1 });
// No Task.FromResult needed
```

## Verify Calls

```csharp
service.Received().Process(Arg.Any<Order>());
service.Received(2).Process(Arg.Any<Order>());
service.DidNotReceive().Cancel(Arg.Any<int>());

// Order matters
Received.InOrder(() =>
{
    service.Validate(order);
    service.Process(order);
});
```

## Raising Events / Exceptions

```csharp
service.GetOrder(1).Returns(x => throw new NotFoundException());

// Raise event
sub.PropertyChanged += Raise.EventWith(new PropertyChangedEventArgs("Name"));
```

**Source:** https://nsubstitute.github.io/help/getting-started/
