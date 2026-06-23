public void Set_ThenGet_ReturnsSetValue()
{
            StoreService store = new StoreService(new InMemoryStore());

            store.Set("key3", "value3");

            Assert.Equal("value3", store.Get("key3"));
        }