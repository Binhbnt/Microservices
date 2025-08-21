using Serilog;
using LeaveRequestService.Extensions;
using System.Data;
using LeaveRequestService.Services; // THÊM DÒNG USING NÀY

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Logger = logger;
builder.Host.UseSerilog(logger);

try
{
    Log.Information("LeaveRequestService is starting up...");
    
    var clientUrl = builder.Configuration["ClientUrl"];
    builder.Services.AddAppServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddHttpClient();
    builder.Services.AddCustomControllers();
    builder.Services.AddCustomHealthChecks(builder.Configuration);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAppOrigin", policy =>
        {
            policy.WithOrigins(clientUrl!).AllowAnyHeader().AllowAnyMethod();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    
    // THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ SERVICE CHẠY NỀN
    builder.Services.AddHostedService<DatabaseInitializerHostedService>();
    
    var app = builder.Build();
    
    app.UseDeveloperSwagger(app.Environment);
    app.UseCors("AllowAppOrigin");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging();
    app.MapControllers();
    app.MapHealthChecks("/health");
    
    // XÓA DÒNG NÀY ĐI
    // app.InitDatabase();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LeaveRequestService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}