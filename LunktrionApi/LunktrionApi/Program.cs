using LunktrionApi.Background;
using LunktrionApi.Data;
using LunktrionApi.Hubs;
using LunktrionApi.Services;
using Microsoft.EntityFrameworkCore;

namespace LunktrionApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        //builder.Services.AddAuthorization();

        var connectionPostgreSQL = builder.Configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrWhiteSpace(connectionPostgreSQL))
        {
            Console.WriteLine("Строка подключения к PostgreSQL не указана");
            throw new InvalidOperationException("Не указана строка подключения к PostgreSQL");
        }

        builder.Services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionPostgreSQL);
        });

        var connectionRedis = builder.Configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(connectionPostgreSQL))
        {
            Console.WriteLine("Строка подключения к Redis не указана");
            throw new InvalidOperationException("Не указана строка подключения к Redis");
        }

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "Lunktrion_";
        });

        builder.Services.AddSingleton<RabbitMqService>();
        builder.Services.AddSingleton<RedisService>();

        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddScoped<MainService>();

        builder.Services.AddHostedService<AppInitializersRunner>();

        builder.Services.AddControllers();

        builder.Services.AddSignalR();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();

            db.Database.Migrate();
        }

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
