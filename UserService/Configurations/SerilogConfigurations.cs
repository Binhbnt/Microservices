using System;
using Serilog;

namespace UserService.Configurations
{
    public static class SerilogConfigurations
    {
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Ghi log từ mức Information trở lên
                .Enrich.FromLogContext()
                .WriteTo.Console() // Ghi ra Console để tiện debug
                .WriteTo.File(
                    path: "Logs/log-.txt", // Ghi vào thư mục Logs
                    rollingInterval: RollingInterval.Day, // Tạo file mới mỗi ngày
                    fileSizeLimitBytes: 1_073_741_824, // Giới hạn 1 GB
                    rollOnFileSizeLimit: true, // Tách file khi đạt giới hạn
                    //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [TraceId: {TraceId}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }
    }
}