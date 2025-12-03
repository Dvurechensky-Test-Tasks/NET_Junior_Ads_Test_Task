/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 декабря 2025 11:18:47
 * Version: 1.0.119
 */

namespace AdRegionService.Services
{
    /// <summary>
    /// Статистика прогрузки рекламных площадок
    /// </summary>
    public class LoadStats
    {
        /// <summary>
        /// Количество загруженных
        /// </summary>
        public int LoadedCount { get; set; }
        /// <summary>
        /// Количество пропущенных
        /// </summary>
        public int SkippedCount { get; set; }
    }
}
