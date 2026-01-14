/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 14 января 2026 06:50:20
 * Version: 1.0.161
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
