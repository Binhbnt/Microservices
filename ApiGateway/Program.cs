using ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Services (DI Container) ---

// 1. Thêm chính sách CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// 2. Thêm các dịch vụ ứng dụng (HealthCheck, MongoDB, SignalR...)
builder.Services.AddAppServices(builder.Configuration);

// 3. Thêm xác thực JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4. Thêm Controllers (nếu Gateway có API riêng như HealthCheck)
builder.Services.AddCustomControllers();

// 5. Thêm cấu hình cho YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

// --- Cấu hình Middleware Pipeline (Thứ tự rất quan trọng) ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 1. Bật định tuyến
app.UseRouting();

// 2. Áp dụng chính sách CORS
// Phải nằm sau UseRouting() và trước UseAuthorization()
app.UseCors("AllowAppOrigin");

// 3. Bật xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// 4. Map các Endpoints
app.UseEndpoints(endpoints =>
{
    // Map các controller của chính Gateway (ví dụ: HealthCheckController)
    endpoints.MapControllers(); 
    // Map các hub SignalR nếu có
    endpoints.MapHub<ApiGateway.Hubs.HealthHub>("/hubs/health"); 
});

// 5. Map Reverse Proxy (YARP) làm bước cuối cùng
app.MapReverseProxy();

app.Run();