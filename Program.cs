
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


            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // 啟用 CORS 政策 -> 這行要放在 UseAuthorization 前面
            app.UseCors("AllowReact");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
