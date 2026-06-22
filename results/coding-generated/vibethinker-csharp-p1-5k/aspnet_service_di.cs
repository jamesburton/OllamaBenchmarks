public static void RegisterOptions(this ServiceCollection services, string sectionName)
{
            services.Configure<OrderSettings>(options =>
            {
                options.WarehouseCode = config["Orders": "WarehouseCode"];
                options.MaxRetries = int.Parse(config["Orders": "MaxRetries"]);
            });
        }