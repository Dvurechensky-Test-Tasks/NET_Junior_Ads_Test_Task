/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 09 ноября 2025 06:50:44
 * Version: 1.0.95
 */

using AdRegionService.Services;

namespace AdRegionService.Endpoints
{
    public static class LoadEndpoints
    {
        public static void MapLoadEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/load", async (IFormFile file, AdPlatformService service, HttpContext ctx) =>
            {
                if (file == null || file.Length == 0)
                    return Results.BadRequest(new { message = "Файл не выбран или пустой" });

                using var stream = file.OpenReadStream();
                await service.LoadFromStreamAsync(stream, ctx.RequestAborted);

                var stats = service.LastLoadStats;

                return Results.Ok(new 
                { 
                    message = "Data loaded", 
                    loaded = stats.LoadedCount,
                    skipped = stats.SkippedCount
                });
            })
            .WithName("UploadAdPlatforms")
            .WithTags("AdPlatforms")
            .Produces(StatusCodes.Status200OK, typeof(object), "application/json")
            .Produces(StatusCodes.Status400BadRequest, typeof(object), "application/json")
            .DisableAntiforgery(); // для .NET 8 (чтобы тестировать в Swagger)
        }
    }
}
