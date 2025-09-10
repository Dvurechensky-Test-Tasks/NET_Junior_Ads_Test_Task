/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 10 сентября 2025 14:28:27
 * Version: 1.0.34
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
