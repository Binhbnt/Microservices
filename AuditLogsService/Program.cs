using Serilog;
using AuditLogService.Extensions;
using AuditLogService.Services; // THÊM DÒNG USING NÀY

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("AuditLogService is starting up...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    
    builder.Services.AddAppServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddCustomControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddCustomHealthChecks(builder.Configuration);
    builder.Services.AddSwaggerGen(); 
    builder.Services.AddCustomCors("AllowAppOrigin", builder.Configuration["ClientUrl"]!);

    // THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ SERVICE CHẠY NỀN
    builder.Services.AddHostedService<DatabaseInitializerHostedService>();

    var app = builder.Build();
    
    app.UseDeveloperSwagger(app.Environment);
    app.UseCors("AllowAppOrigin");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // XÓA DÒNG NÀY ĐI
    // app.InitDatabase(); 
    
    app.MapGet("/", () => "AuditLogService is running...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("CRITICAL ERROR: " + ex.Message); 
    Console.WriteLine(ex.StackTrace);               
    Log.Fatal(ex, "AuditLogService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}