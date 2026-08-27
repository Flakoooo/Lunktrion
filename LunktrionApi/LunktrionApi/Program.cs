using LunktrionApi.Hubs;
using LunktrionApi.Services;

namespace LunktrionApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        //builder.Services.AddAuthorization();

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "Lunktrion_";
        });

        builder.Services.AddSingleton<RabbitMqService>();
        builder.Services.AddSingleton<RedisService>();

        builder.Services.AddSingleton<DeviceRegistry>();
        builder.Services.AddScoped<MainService>();

        builder.Services.AddControllers();

        builder.Services.AddSignalR();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapHub<MainHub>("/mainHub");

        app.Run();
    }
}
