/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 07 апреля 2026 10:54:25
 * Version: 1.0.244
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
