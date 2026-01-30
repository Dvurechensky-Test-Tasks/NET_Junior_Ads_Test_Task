/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 30 января 2026 06:50:21
 * Version: 1.0.177
 */

using AdRegionService.Endpoints;

namespace AdRegionService.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapLoadEndpoints();
            app.MapSearchEndpoints();
        }
    }
}
