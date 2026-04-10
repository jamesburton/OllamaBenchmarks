public async Task Consume(ConsumeContext<GetProduct> context) {
                if (context.Message.ProductId == ownKnownId) {
                    await context.RespondAsync(new ProductResult(KnownId, "Test Widget", 19.99m));
                } else {
                    await context.RespondAsync(new ProductNotFound());
                }
’            }