/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 21 декабря 2025 16:08:24
 * Version: 1.0.137
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
