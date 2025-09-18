﻿/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 18 сентября 2025 06:50:21
 * Version: 1.0.42
 */

using AdRegionService.Models;
using AdRegionService.Services;

namespace AdRegionService.Endpoints
{
    public static class SearchEndpoints
    {
        public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/search", (string location, AdPlatformService service) =>
            {
                var result = service.Search(location);
                return Results.Ok(result);
            })
            .WithName("SearchAdPlatforms")
            .WithTags("AdPlatforms")
            .Produces(StatusCodes.Status200OK, typeof(List<AdPlatform>))
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(o =>
            {
                // Пример query-параметра
                o.Parameters[0].Description = "Локация для поиска рекламных площадок";
                return o;
            });
        }
    }
}
