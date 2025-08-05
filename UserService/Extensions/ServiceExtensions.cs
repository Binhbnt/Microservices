// Extensions/ServiceExtensions.cs
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using UserService.Repositories;
using UserService.Services;
using UserService.Data;
using Npgsql;

namespace UserService.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // 1. Kết nối DB
            var connectionString = config.GetConnectionString("DefaultConnection");
            //services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));
            //services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
            // 2. AutoMapper
            services.AddAutoMapper(typeof(Program));

            // 3. Repositories & Services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddHttpContextAccessor();
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

        // public static void InitDatabase(this IApplicationBuilder app)
        // {
        //     using var scope = app.ApplicationServices.CreateScope();
        //     var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        //     new DatabaseInitializer(db).Initialize();
        // }

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

        public static void AddCustomCors(this IServiceCollection services, string policyName, string clientUrl)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    policy.WithOrigins(clientUrl) // Cho phép frontend của bạn được gọi API
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
        public static void AddCustomHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthChecks()
                .AddNpgSql(config.GetConnectionString("DefaultConnection"), name: "PostgreSQL");
        }
    }
}
