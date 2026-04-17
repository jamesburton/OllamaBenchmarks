using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenancy
{
    // --- Models ---

    public record TenantInfo(string string TenantId, string Name, string Plan, bool IsActive);

    // --- Abstractions ---

    public interface ITenantResolver
    {
        Task<TenantInfo?> ResolveAsync(HttpContext ctx, CancellationToken ct);
    }

    public interface ITenantStore
    {
        Task<TenantInfo?> GetAsync(string tenantId, CancellationToken ct);
        Task RegisterAsync(TenantInfo tenant, CancellationToken ct);
    }

    // --- Infrastructure ---

    public class HeaderTenantResolver : ITenantResolver
    {
        private readonly ITenantStore _tenantStore;

        public HeaderTenantResolver(ITenantStore tenantStore)
        {
            _tenantStore = tenantStore;
        }

        public async Task<TenantInfo?> ResolveAsync(HttpContext ctx, CancellationToken ct)
        {
            if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
            {
                var tenantId = tenantIdValues.ToString();
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    return await _tenantStore.GetAsync(tenantId, ct);
                }
            }
            return null;
        }
    }

    public class InMemoryTenantStore : ITenantStore
    {
        private readonly Dictionary<string, TenantInfo> _tenants = new(StringComparer.OrdinalIgnoreCase);

        public InMemoryTenantStore()
        {
            // Seed data
            var tenant = new TenantInfo("tenant-001", "Acme Corp", "Enterprise", true);
            _tenants[tenant.TenantId] = tenant;
        }

        public Task<TenantInfo?> GetAsync(string tenantId, CancellationToken ct)
        {
            _tenants.TryGetValue(tenantId, out var tenant);
            return Task.FromResult(tenant);
        }

        public Task RegisterAsync(TenantInfo tenant, CancellationToken ct)
        {
            _tenants[tenant.TenantId] = tenant;
            return Task.CompletedTask;
        }
    }

    // --- Middleware ---

    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantResolver _resolver;

        public TenantMiddleware(RequestDelegate next, ITenantResolver resolver)
        {
            _next = next;
            _resolver = resolver;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var tenant = await _resolver.ResolveAsync(ctx, ctx.RequestAborted);

            if (tenant is null)
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (!tenant.IsActive)
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.WriteAsync("Tenant is inactive.");
                return;
            }

            ctx.Items["Tenant"] = tenant;

            await _next(ctx);
        }
    }

    // --- Extensions ---

    public static class MultiTenancyExtensions
    {
        public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
        {
            services.AddSingleton<ITenantStore, InMemoryTenantStore>();
            services.AddScoped<ITenantResolver, HeaderTenantResolver>();
            return services;
        }

        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantMiddleware>();
        }
    }
}