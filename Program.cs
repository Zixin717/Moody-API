
using Microsoft.EntityFrameworkCore;
using Moody_backend.Data;

namespace Moody_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 新增 CORS 政策 -> 允許 React 存取 API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReact", policy =>
                {
                    // 這裡填寫我 React 啟動時的網址 -> 通常 Vite 是 5173，請依我實際的 port 為準。
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            ); // 在 builder.Services.AddControllers(); 的前面加上
            builder.Services.AddControllers();


            // 換掉原本的 AddOpenApi()，改用這個
            builder.Services.AddEndpointsApiExplorer();   // 讓 Swagger 能掃描你的 Controller
            builder.Services.AddSwaggerGen();             // 產生 Swagger 規格 + UI
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // app.MapOpenApi();
                app.UseSwagger();       // 產生 /swagger/v1/swagger.json
                app.UseSwaggerUI();     // 啟動 UI 介面，網址是 /swagger
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowReact");  // 啟用 CORS 政策 -> 這行要放在 UseAuthorization 前面
            app.UseStaticFiles();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
