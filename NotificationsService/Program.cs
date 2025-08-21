using Serilog;
using System.Data;
using NotificationsService.Extensions;
using NotificationsService.Hubs;
using Microsoft.AspNetCore.SignalR;
using NotificationsService.Services; // THÊM DÒNG USING NÀY

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Gán logger vừa tạo cho Log.Logger toàn cục để có thể dùng ngay.
Log.Logger = logger;

// Gắn logger đã tạo vào Host.
builder.Host.UseSerilog(logger);
try
{
    Log.Information("NotificationService is starting up...");

    // === 3. Đăng ký các dịch vụ ===
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
            policy.WithOrigins(clientUrl!)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddSignalR();
    
    // THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ SERVICE CHẠY NỀN
    builder.Services.AddHostedService<DatabaseInitializerHostedService>();
    
    // === 4. Build ứng dụng ===
    var app = builder.Build();

    // === 5. Middlewares ===
    app.UseDeveloperSwagger(app.Environment);
    app.UseCors("AllowAppOrigin");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging(); // Ghi log cho mỗi request HTTP
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHub<NotificationsService.Hubs.NotificationHub>("/notificationHub");
    
    // === 6. Khởi tạo CSDL (XÓA DÒNG NÀY ĐI) ===
    // app.InitDatabase();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NotificationService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}