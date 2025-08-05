using System.Data;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApiGateway.Interface;
using ApiGateway.Services;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using ApiGateway.Hubs;

namespace ApiGateway.Extensions;

public static class ServiceExtensions
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        // DB nội bộ nếu có
        services.AddScoped<IDbConnection>(_ => new SqliteConnection(config.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(typeof(Program));
        //services.AddScoped<IHealthCheckService, HealthCheckService>();
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

    public static void AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
        });
    }

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origin = config["ClientUrl"] ?? "http://localhost:5173";
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAppOrigin", builder =>
            {
                 builder.WithOrigins(origin, "http://localhost:5173")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");

        var secret = jwtSettings["Secret"] ?? "WOVFdTnjIB5ZQf4bvIhp5BmSsfTrBNC2wXwM0LIW7PE=";
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

    public static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                    Enter 'Bearer' [space] and then your token in the text input below.
                    Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
            });
        });
    }
    public static void UseAppEndpoints(this IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<HealthHub>("/hubs/health");
        });
    }
}
