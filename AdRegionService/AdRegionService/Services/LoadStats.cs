/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 января 2026 15:45:13
 * Version: 1.0.152
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
