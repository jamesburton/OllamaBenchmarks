using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using AwesomeAssertions;

namespace StoreServiceTests
{
    public class InMemoryStore : IAsyncLifetime
    {
        public Dictionary<string, string> Data { get; private set; } = new();

        public async ValueTask InitializeAsync()
        {
            Data.Clear();
            Data["key1"] = "value1";
            Data["key2"] = "value2";
            await ValueTask.FromResult(0);
        }

        public async ValueTask DisposeAsync()
        {
            Data.Clear();
            await ValueTask.FromResult(0);
        }
    }

    public class StoreService
    {
        private readonly InMemoryStore _store;

        public StoreService(InMemoryStore store)
        {
            _store = store;
        }

        public string? Get(string key)
        {
            return _store.Data.TryGetValue(key, out var value) ? value : null;
        }

        public void Set(string key, string value)
        {
            _store.Data[key] = value;
        }
    }

    public class StoreServiceTests
    {
        private readonly InMemoryStore _store;
        private readonly StoreService _service;

        public StoreServiceTests()
        {
            _store = Substitute.For<InMemoryStore>();
            _service = new StoreService(_store);
        }

        [Fact]
        public void Get_ExistingKey_ReturnsValue()
        {
            // Arrange
            _store.Data["key1"] = "value1";

            // Act
            var result = _service.Get("key1");

            // Assert
            result.Should().Be("value1");
        }

        [Fact]
        public void Set_ThenGet_ReturnsSetValue()
        {
            // Arrange
            _service.Set("key3", "value3");

            // Act
            var result = _service.Get("key3");

            // Assert
            result.Should().Be("value3");
        }

        [Fact]
        public void Get_NonexistentKey_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent");

            // Assert
            result.Should().BeNull();
        }
    }
}