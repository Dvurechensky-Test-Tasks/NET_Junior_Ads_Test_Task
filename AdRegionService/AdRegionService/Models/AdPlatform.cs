/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 07 января 2026 10:22:14
 * Version: 1.0.154
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
