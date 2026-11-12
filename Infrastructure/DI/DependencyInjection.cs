using Infrastructure.Auth.Interfaces;
using Infrastructure.Auth.Services;
using Infrastructure.BackgroundTasks;
using Infrastructure.Data;
using Infrastructure.Email.Interfaces;
using Infrastructure.Email.Services;
using Infrastructure.Products.Interfaces;
using Infrastructure.Products.Repositories;
using Infrastructure.Products.Services;
using Infrastructure.RedisCache;
using Infrastructure.RedisCache.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");

        services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

        if (configuration.GetValue("Caching:UseRedis", false))
        {
            var redis = configuration.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(redis))
                throw new InvalidOperationException("Caching:UseRedis requires ConnectionStrings:Redis.");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis;
                options.InstanceName = "marketing-manager:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();

        if (configuration.GetValue("BackgroundTasks:CountProductsEnabled", true))
            services.AddHostedService<ProductCountBackgroundService>();

        return services;
    }
}
