/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 20 августа 2025 02:06:41
 * Version: 1.0.
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

        var app = builder.Build();

        // Middlewares
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Endpoints - конечные точки
        app.MapProjectEndpoints();

        app.Run();

    }
}