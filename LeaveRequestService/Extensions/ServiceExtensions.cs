using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using LeaveRequestService.Data;
using LeaveRequestService.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LeaveRequestService.Interface;
using LeaveRequestService.Services;
using Npgsql;

namespace LeaveRequestService.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // DÒNG NÀY ĐÃ ĐƯỢC XÓA
            // services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(config.GetConnectionString("DefaultConnection")));

            services.AddAutoMapper(typeof(Program));
            
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            services.AddScoped<ILeaveRequestManagementService, LeaveRequestManagementService>();
            services.AddScoped<IUserServiceClient, UserServiceClient>();
            
            services.AddHttpContextAccessor();
            services.AddHttpClient("UserClient", client =>
            {
                client.BaseAddress = new Uri(config["ServiceUrls:ApiGatewayApi"]!);
            });
            services.AddHttpClient("AuditLogClient", client =>
            {
                client.BaseAddress = new Uri(config["ServiceUrls:ApiGatewayApi"]!);
            });
            services.AddHttpClient("NotificationClient", client =>
            {
                client.BaseAddress = new Uri(config["ServiceUrls:ApiGatewayApi"]!);
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
                // Sửa Title cho đúng service
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Leave Request Service API", Version = "v1" });
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
}