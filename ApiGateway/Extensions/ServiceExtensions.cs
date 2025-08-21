using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ApiGateway.Hubs;
using ApiGateway.Interface;
using ApiGateway.Services;

namespace ApiGateway.Extensions;

public static class ServiceExtensions
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAutoMapper(typeof(Program));
        services.AddSingleton<IHealthCheckService, HealthCheckService>();
        services.AddHostedService<HealthLogCleanupService>();
        services.AddHostedService<HealthCheckScheduler>();
        services.AddSignalR();
        
        // MongoDB cho HealthCheck
        services.AddSingleton<IMongoClient>(sp => 
            new MongoClient(config["MongoDB:ConnectionString"]));
    }

    public static void AddCustomControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(opt =>
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
    
    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origin = config["ClientUrl"];
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAppOrigin", builder =>
            {
                // Cho phép cả origin từ config và localhost (khi dev)
                builder.WithOrigins(origin!, "http://localhost:5173") 
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // Quan trọng cho SignalR
            });
        });
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? "fallback_secret_key_should_be_long_and_complex";
        var key = Encoding.ASCII.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }
    
}