using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using AwesomeAssertions;

namespace CounterTests
{
    public class CounterService
    {
        private int _count = 0;

        public void Increment()
        {
            _count++;
        }

        public void Reset()
        {
            _count = 0;
        }
    }

    [CollectionDefinition("Counter")]
    internal class CounterCollection : ICollectionFixture<CounterFixture>
    {
        // No test methods here, just the attribute and interface
    }

    public class CounterFixture : IAsyncLifetime
    {
        private readonly CounterService _counter;

        public CounterFixture()
        {
            _counter = new CounterService();
        }

        public ValueTask InitializeAsync()
        {
            return default;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }

    [Collection("Counter")]
    public class CounterFirstTests
    {
        private readonly CounterFixture _fixture;

        public CounterFirstTests(CounterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ShouldHaveAtLeastOne()
        {
            // Arrange
            _fixture.Counter.Increment();

            // Act

            // Assert
            _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Collection("Counter")]
    public class CounterSecondTests
    {
        private readonly CounterFixture _fixture;

        public CounterSecondTests(CounterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ShouldHaveAtLeastTwo()
        {
            // Arrange

            // Act

            _fixture.Counter.Increment();
            _fixture.Counter.Increment();

            // Assert
            _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
        }
    }
}