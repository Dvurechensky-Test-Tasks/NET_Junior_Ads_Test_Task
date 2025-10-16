/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 16 октября 2025 10:56:33
 * Version: 1.0.70
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
