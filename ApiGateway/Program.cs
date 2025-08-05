using ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerWithJwt(); 
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomControllers();

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseCors("AllowAppOrigin");
app.UseAppEndpoints();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/users/swagger/v1/swagger.json", "User Service");
        options.RoutePrefix = "swagger";
    });
}

app.MapReverseProxy();

app.Run();
