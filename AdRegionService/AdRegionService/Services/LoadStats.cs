/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 24 марта 2026 08:52:31
 * Version: 1.0.230
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
