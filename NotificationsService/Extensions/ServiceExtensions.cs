using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationsService.Data;
using NotificationsService.Interface;
using NotificationsService.Repositories;
using NotificationsService.Services;
using Npgsql;

namespace NotificationsService.Extensions;

public static class ServiceExtensions
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        // XÓA DÒNG AddScoped<IDbConnection> Ở ĐÂY
        services.AddAutoMapper(typeof(Program));
        
        services.AddScoped<IAppNotificationRepository, AppNotificationRepository>();
        services.AddScoped<IAppNotificationService, AppNotificationService>();
        services.AddSingleton<IUserIdProvider, Services.NameIdentifierUserIdProvider>();
        
        services.AddHttpContextAccessor();
        services.AddHttpClient("UserClient", client =>
        {
            client.BaseAddress = new Uri(config["ServiceUrls:UserApi"]!);
        });
        services.AddHttpClient("AuditLogClient", client =>
        {
            client.BaseAddress = new Uri(config["ServiceUrls:AuditLogApi"]!);
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
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/notificationHub")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
    }

    public static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notifications Service API", Version = "v1" }); // Sửa Title
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

    // PHƯƠNG THỨC InitDatabase ĐÃ ĐƯỢC XÓA HOÀN TOÀN

    public static void UseDeveloperSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }

    public static void AddCustomControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(o =>
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    public static void AddCustomHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddNpgSql(config.GetConnectionString("DefaultConnection"), name: "PostgreSQL");
    }
}