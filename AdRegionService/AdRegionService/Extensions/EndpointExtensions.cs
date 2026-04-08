/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 08 апреля 2026 14:25:25
 * Version: 1.0.245
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
