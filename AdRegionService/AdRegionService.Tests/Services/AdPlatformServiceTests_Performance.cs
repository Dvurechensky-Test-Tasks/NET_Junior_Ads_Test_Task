/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 09 декабря 2025 13:31:53
 * Version: 1.0.125
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AdRegionService.Models;
using AdRegionService.Services;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace AdRegionService.Tests.Services
{
    public class AdPlatformServiceTests_Performance
    {
        private static IReadOnlyList<AdPlatform> GetPlatforms(AdPlatformService svc)
        {
            var field = svc.GetType().GetField("_platforms", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IReadOnlyList<AdPlatform>)field?.GetValue(svc);
        }

        #region LoadFromStreamAsync
        
        /// <summary>
        /// Жёсткий Performance тест: 1 млн площадок × 5 локаций каждая за менее 20 секунд
        /// Проверяет корректность загрузки и скорость работы.
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        public async Task LoadFromStreamAsync_HardPerformanceTest()
        {
            const int platformCount = 1_000_000;
            const int locationsPerPlatform = 5;

            // Генерация входного файла в памяти
            var lines = new List<string>(platformCount);
            for (int i = 0; i < platformCount; i++)
            {
                var locs = Enumerable.Range(0, locationsPerPlatform)
                                     .Select(j => $"/loc{i}_{j}");
                lines.Add($"P{i}:{string.Join(',', locs)}");
            }

            var content = string.Join('\n', lines);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var svc = new AdPlatformService(NullLogger<AdPlatformService>.Instance);

            // Замер времени загрузки
            var sw = Stopwatch.StartNew();
            await svc.LoadFromStreamAsync(stream);
            sw.Stop();

            var list = GetPlatforms(svc);

            // Проверка корректности
            Assert.Equal(platformCount, list.Count);
            Assert.All(list, p => Assert.Equal(locationsPerPlatform, p.Locations.Count));

            // Проверка скорости: ограничение, допустим, 20 секунд на такой объём
            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(20),
                $"Парсинг занял слишком долго: {sw.Elapsed}.");

            // Дополнительно проверим крайние элементы
            var first = list.First();
            var last = list.Last();
            Assert.Equal("P0", first.Name);
            Assert.Equal($"P{platformCount - 1}", last.Name);
        }

        #endregion
    }
}
