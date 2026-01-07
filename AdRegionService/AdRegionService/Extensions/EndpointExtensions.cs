/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 07 января 2026 10:22:14
 * Version: 1.0.154
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
