using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// IOrderRepository interface
public interface IOrderRepository
{
}

// OrderRepository class implementing IOrderRepository
public class OrderRepository : IOrderRepository
{
}

// IOrderService interface
public interface IOrderService
{
}

// OrderService class implementing IOrderService
public class OrderService : IOrderService
{
}

// IEmailNotifier interface
public interface IEmailNotifier
{
}

// EmailNotifier class implementing IEmailNotifier
public class EmailNotifier : IEmailNotifier
{
}

// OrderSettings class with properties: string WarehouseCode, int MaxRetries
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// ServiceCollectionExtensions static class with an extension method
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        services.Configure<OrderSettings>(config.GetSection("Orders"));

        return services;
    }
}

// Example usage
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddHttpClient<IGitHubClient, GitHubClient>(c =>
            c.BaseAddress = new Uri("https://api.github.com/"));

        // Add order services
        var appSettings = builder.Configuration.GetSection("Orders").Get<OrderSettings>();
        builder.Services.AddOrderServices(appSettings);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}