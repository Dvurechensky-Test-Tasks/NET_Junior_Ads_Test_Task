/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 19 октября 2025 13:16:49
 * Version: 1.0.73
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
