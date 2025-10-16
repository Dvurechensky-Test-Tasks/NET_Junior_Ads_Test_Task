/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 16 октября 2025 10:56:33
 * Version: 1.0.70
 */

namespace AdRegionService.Models
{
    /// <summary>
    /// Рекламная площадка
    /// </summary>
    public class AdPlatform
    {
        /// <summary>
        /// Имя площадки
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Локация площадки
        /// </summary>
        public List<string> Locations { get; set; } = new();
    }
}
