using Serilog;
using Serilog.Sinks.File;
using Microsoft.Extensions.Configuration;
using UserService.Extensions;
using UserService.Data;
using UserService.Services; // THÊM DÒNG USING NÀY

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("UserService is starting up...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var clientUrl = builder.Configuration["ClientUrl"];
    
    builder.Services.AddAppServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddCustomControllers();
    builder.Services.AddCustomCors("AllowAppOrigin", clientUrl!);
    builder.Services.AddCustomHealthChecks(builder.Configuration);
    
    // THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ SERVICE CHẠY NỀN
    builder.Services.AddHostedService<DatabaseInitializerHostedService>();
    
    var app = builder.Build();

    app.UseDeveloperSwagger(app.Environment);
    app.UseStaticFiles();
    
    app.UseCors("AllowAppOrigin");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // XÓA DÒNG NÀY ĐI
    // app.InitDatabase(); 

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "UserService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}