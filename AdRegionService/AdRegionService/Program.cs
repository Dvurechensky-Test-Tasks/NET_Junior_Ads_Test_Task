/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 01 сентября 2025 11:04:46
 * Version: 1.0.25
 */

using AdRegionService.Extensions;
using AdRegionService.Services;

namespace AdRegionService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Сервисы
        builder.Services.AddSingleton<AdPlatformService>();

        // Swagger - документация
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Для Docker (HTTPS) - разрешаем все запросы с любых источников
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Редирект с "/" на Swagger
        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger");
            return Task.CompletedTask;
        });

        // Middlewares
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdRegionService API V1");
        });

        // Для Docker (HTTPS)
        app.UseCors();

        // Endpoints - конечные точки
        app.MapProjectEndpoints();

        app.Run();
    }
}