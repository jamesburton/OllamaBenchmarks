# AwesomeAssertions v9.4.0 — Quick Reference

**Package:** `AwesomeAssertions`
**Namespace:** `using AwesomeAssertions;`
**Note:** Fork of FluentAssertions — identical API, different namespace.

## Basic Assertions

```csharp
result.Should().Be(42);
result.Should().NotBe(0);
str.Should().Be("hello").And.HaveLength(5);
obj.Should().BeNull();
obj.Should().NotBeNull();
```

## Type Assertions

```csharp
obj.Should().BeOfType<Order>();
obj.Should().BeAssignableTo<IEntity>();
```

## Exception Assertions

```csharp
Action act = () => sut.DoThing(null);
act.Should().Throw<ArgumentNullException>()
   .WithMessage("*param*");

Func<Task> asyncAct = () => sut.GetAsync(-1);
await asyncAct.Should().ThrowAsync<NotFoundException>();
```

## Collections

```csharp
list.Should().HaveCount(3);
list.Should().Contain(x => x.Id == 1);
list.Should().OnlyContain(x => x.IsActive);
list.Should().BeInAscendingOrder(x => x.Name);
list.Should().BeEmpty();
```

## Object Graph Equivalence

```csharp
// Deep equality, ignoring Id field
actual.Should().BeEquivalentTo(expected,
    opt => opt.Excluding(x => x.Id));

// Collection equivalence (order-insensitive by default)
actualList.Should().BeEquivalentTo(expectedList);
```

**Source:** https://awesomeassertions.org/introduction
