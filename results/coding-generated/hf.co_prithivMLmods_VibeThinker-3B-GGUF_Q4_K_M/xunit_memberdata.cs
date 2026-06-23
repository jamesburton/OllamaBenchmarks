public struct TestCase : struct
{
    public int n;
    public bool expected;
}

var data = MathValidator.EvenData;
var case = data[0]; // maybe
assert MathValidator.IsEven(case.n).Should().Be(case(expected));

var case = MathValidator.EvenData[i];
MathValidator.IsEven(case.n).Should().Be(case(expected));

[Theory]
public void IsEven_WithMemberData(int n, bool expected)
{
    var data = MathValidator.EvenData;
    var case = data[(int)n];
    MathValidator.IsEven(n).Should().Be(case(expected));
}

result Should().Be(42);
result Should().NotBe(0);
str Should().Be("hello").And.HaveLength(5);
obj Should().IsNull();
obj Should().NotNull();

obj Should().Is instance of Order>();
obj Should().IsAssignableTo<IEntity>();

action Should().Throw<ArgumentNullException>() .WithMessage("*param*");

list Should().HaveCount(3);
list Should().Contain(x => x.Id == 1);
list Should().OnlyContain(x => x.Active);
list Should().IsSorted(AscendingOrder(x => x.Name));
list Should().IsEmpty();

actual Should().BeEquivalentTo(expected, opt => opt.Excluding(x => x.Id));
actualList Should().BeEquivalentTo(expectedList);

var result = s.t; // some object
Assert result.IsEqual(42); // Not sure.

But from docs: Basic assertions: `result Should().Be(42)`. Actually the doc shows "result Should().Be(42);" So they use `.Be()` method? Let's search memory. I recall that FluentAssertions uses `.Should().IsEqual(value)`, but Awesomeasserts may have similar.

Let’s check typical usage from awesomeasserts: Example from docs:


I think `.Be` is a method defined in Awesomeasserts that calls the internal `Assert.Equal`. Indeed earlier they gave "Basic Assertions" list includes `result Should().Be(42);`. So we should use `.Be()`.

Thus for test:

`MathValidator.IsEven(n).Should().Be(caseExpected)`.

But note: The property returns a struct class with fields n and expected. We'll call case = data[(int)n]; then case.n is the same as n but we could just use `case(expected)`. Actually we can store both values in the array element, so we can directly compare `MathValidator.IsEven(case.n).Should().Be(caseexpected)`. Or simpler: compute expected boolean and pass to `.Be`.

Thus test:


But we need to use `.Should().Be` method. Actually the syntax is `expected.Should().Be(value)`. Wait, we have:

`MathValidator.IsEven(n)` returns bool. Then we chain `.Should().Be(expected)`. So it's:

`MathValidator.IsEven(n).Should().Ave? No.

Let's check the doc: "result Should().Be(42);" That means after obtaining a result (like variable), you call `.Should()` method, then `.Be(42)`. So we need to write:

`MathValidator.IsEven(n).Should().Be(expected)`? Actually it's `.Be`, not `.Be`. The method is called `Be`. In C# naming conventions, they would use `Be`. So the chain is: `MathValidator.IsEven(n).Should().Be(expected)`. But in the doc they wrote "result Should().Be(42);" Not "result Should().Be". So it's a static method? Let's examine typical usage:

If we have variable `bool result = ...;` then you would write:

`result Should().Be(42);`

Thus the chain is: `result.Should().Be(value)`. So we need to use `.Be` (lowercase). Actually they wrote "Be" capitalized? In the doc snippet it's "Be". It might be case-sensitive: The method name is `Be`. Usually in C# method names are PascalCase, so `Be` is correct. So chain: `MathValidator.IsEven(n).Should().Be(expected)`.

Thus test should use `.Should().Be(expected)`. Wait we have two 'E's? Actually it's `.Should().Be` vs `.Should().Be`. Let's check the doc again:


The dot after Should is `.` then method name `Be`. So chain: `result.Should().Be(42)`. There's no second dot before Be; it's just one dot between Should and Be because method chaining uses dot to call next method? Actually typical C# chaining would be `obj.Should().IsEqual(value);` where `.Should()` returns an assertion object, then you chain `.IsEqual`. In FluentAssertions they use `.Should().IsEqual`. For Awesomeasserts they might have `.Be` as extension method that inherits from `FluentAssert`. Actually they may combine both: `.Should().Be` maybe.

Let's search memory: I recall using xUnit 3 with FluentAssertions: you would write:


But in code, you'd need to use `Assert` explicitly? Actually the fluent assertions are extension methods defined in the `Assert` class. So you call like:

`resultShould().Be(42);` maybe.

Let's check xUnit v3 documentation: In xUnit 3, they introduced a new assertion API where you can just write: