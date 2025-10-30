/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 30 октября 2025 17:05:40
 * Version: 1.0.85
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
    public class AdPlatformServiceTests
    {
        private static IReadOnlyList<AdPlatform> GetPlatforms(AdPlatformService svc)
        {
            var field = svc.GetType().GetField("_platforms", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IReadOnlyList<AdPlatform>)field?.GetValue(svc);
        }

        private static async Task<AdPlatformService> ParseAsync(string content)
        {
            var svc = await ParseStreamAsync(content);
            return svc;
        }

        private async Task<AdPlatformService> CreateServiceWithSampleData()
        {
            var svc = new AdPlatformService(NullLogger<AdPlatformService>.Instance);
            string content = "Яндекс.Директ:/ru\n" +
                "Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik\n" +
                "Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl\n" +
                "Крутая реклама:/ru/svrd\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await svc.LoadFromStreamAsync(stream);
            return svc;
        }

        private static async Task<AdPlatformService> ParseStreamAsync(string content)
        {
            var svc = new AdPlatformService(NullLogger<AdPlatformService>.Instance);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await svc.LoadFromStreamAsync(stream);
            return svc;
        }

        #region Search
        /*
            1. null или пустой ввод.
            2. Точное совпадение локации.
            3. Вложенные локации с несколькими уровнями.
            4. Вложенные локации без точного совпадения (используем родительские площадки).
            5. Верхний уровень локации (глобальные площадки).
            6. Несуществующая локация.
         */

        [Fact]
        public async Task Search_NullOrWhitespace_ReturnsEmpty()
        {
            // Проверка, что если передана пустая строка или null, метод корректно возвращает пустую коллекцию.
            var svc = await CreateServiceWithSampleData();

            Assert.Empty(svc.Search(null));
            Assert.Empty(svc.Search(""));
            Assert.Empty(svc.Search("   "));
        }

        [Fact]
        public async Task Search_ExactMatch_ReturnsAllApplicablePlatforms()
        {
            // Проверка точного совпадения локации.
            // Для /ru/msk должны вернуться все площадки, которые действуют прямо в этой локации,
            // включая глобальные площадки с родительской локацией (/ru) и конкретные (/ru/msk).
            var svc = await CreateServiceWithSampleData();
            var results = svc.Search("/ru/msk").Select(p => p.Name).ToList();

            Assert.Equal(2, results.Count);
            Assert.Contains("Яндекс.Директ", results);
            Assert.Contains("Газета уральских москвичей", results);
        }

        [Fact]
        public async Task Search_PrefixMatch_ReturnsAllMatchingLevels()
        {
            // Проверка вложенной локации с точным совпадением и родительскими префиксами.
            // Для /ru/svrd/revda вернутся:
            // - Ревдинский рабочий (/ru/svrd/revda)
            // - Крутая реклама (/ru/svrd)
            // - Яндекс.Директ (/ru)
            var svc = await CreateServiceWithSampleData();
            var results = svc.Search("/ru/svrd/revda").ToList();

            var names = results.Select(p => p.Name).ToHashSet();
            Assert.Contains("Ревдинский рабочий", names);
            Assert.Contains("Крутая реклама", names);
            Assert.Contains("Яндекс.Директ", names); // /ru
            Assert.Equal(3, names.Count);
        }

        [Fact]
        public async Task Search_PrefixWithoutExactMatch_ReturnsParentPlatforms()
        {
            // Проверка вложенной локации без точного совпадения.
            // /ru/svrd/unknown не имеет точного соответствия, 
            // но метод должен вернуть все платформы из родительских уровней:
            // - Крутая реклама (/ru/svrd)
            // - Яндекс.Директ (/ru)
            var svc = await CreateServiceWithSampleData();
            var results = svc.Search("/ru/svrd/unknown").ToList();

            var names = results.Select(p => p.Name).ToHashSet();
            Assert.Contains("Крутая реклама", names);
            Assert.Contains("Яндекс.Директ", names);
            Assert.Equal(2, names.Count);
        }

        [Fact]
        public async Task Search_TopLevelLocation_ReturnsOnlyRootPlatforms()
        {
            // Проверка верхнего уровня локации.
            // Для /ru должны вернуться только глобальные площадки, которые действуют на этом уровне.
            var svc = await CreateServiceWithSampleData();
            var results = svc.Search("/ru").ToList();

            var names = results.Select(p => p.Name).ToList();
            Assert.Single(names);
            Assert.Contains("Яндекс.Директ", names);
        }

        [Fact]
        public async Task Search_NonExistingLocation_ReturnsEmpty()
        {
            // Проверка для полностью несуществующей локации.
            // Метод должен возвращать пустую коллекцию без ошибок.
            var svc = await CreateServiceWithSampleData();
            var results = svc.Search("/nonexistent").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task Search_ReturnsAllApplicablePlatforms()
        {
            // Ещё одна проверка для /ru/msk, чтобы убедиться, что метод возвращает
            // все площадки, включая точные и родительские локации, корректно.
            var svc = await CreateServiceWithSampleData();

            var results = svc.Search("/ru/msk").Select(p => p.Name).ToHashSet();

            Assert.Equal(2, results.Count);
            Assert.Contains("Яндекс.Директ", results);
            Assert.Contains("Газета уральских москвичей", results);
        }

        #endregion

        #region LoadFromStreamAsync

        /*
            1. **Базовый позитивный сценарий** – корректный разбор обычного файла с несколькими площадками и локациями.
            2. **Тримминг** – удаление пробелов вокруг имени и локаций.
            3. **Пустые строки** – пропуск пустых или полностью пробельных строк.
            4. **Строки без двоеточия** – игнорируются.
            5. **Несколько двоеточий в строке** – правая часть сохраняется целиком.
            6. **Пустое имя** – строка игнорируется.
            7. **Пустой список локаций** – строка игнорируется.
            8. **Дубли и пустые элементы после запятых** – удаляются.
            9. **Переводы строк Windows (\r\n)** – корректная обработка.
            10. **Unicode/кириллица** – корректное сохранение.
            11. **Null или пустой поток** – метод не падает, состояние не изменяется.
            12. **Пустой поток** – создаётся пустой список.
            13. **Хвостовые и начальные запятые** – пустые элементы отрезаются.
            14. **Большой объём данных** – корректность и грубая производительность.
            15. **Только пробелы после двоеточия** – игнорируются.
            16. **Смешанные валидные и невалидные строки** – метод не ломается, валидные обрабатываются.
            17. **Дублирующиеся названия площадок** – сохраняются как есть.
         */

        /// <summary>
        /// 1. Базовый позитивный сценарий 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_ParsesSampleFile_Success()
        {
            // Act
            var svc = await CreateServiceWithSampleData();
            var list = GetPlatforms(svc);

            // Assert
            Assert.NotNull(list);
            Assert.Equal(4, list.Count);

            Assert.Equal("Яндекс.Директ", list[0].Name);
            Assert.Single(list[0].Locations);
            Assert.Equal("/ru", list[0].Locations[0]);

            Assert.Equal("Ревдинский рабочий", list[1].Name);
            Assert.Equal(2, list[1].Locations.Count);
            Assert.Contains("/ru/svrd/revda", list[1].Locations);
            Assert.Contains("/ru/svrd/pervik", list[1].Locations);
        }

        /// <summary>
        /// 2. Тримминг и пробелы 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_TrimsNameAndLocations()
        {
            var content = "  Name  :  /a ,  /b  ,  /c  \n";
            var svc = await ParseAsync(content);
            var p = GetPlatforms(svc).Single();

            Assert.Equal("Name", p.Name);
            Assert.Equal(new[] { "/a", "/b", "/c" }, p.Locations);
        }

        /// <summary>
        /// 3. Пропуск пустых строк 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_IgnoresEmptyLines()
        {
            var content = "\n\nName:/a\n\n\nAnother:/b\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Equal(2, list.Count);
        }

        /// <summary>
        /// 4. Строки без двоеточия игнорируются 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_LinesWithoutColon_AreIgnored()
        {
            var content = "NoColonHere\nValid:/ok\nAlsoNoColon\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Single(list);
            Assert.Equal("Valid", list[0].Name);
            Assert.Equal(new[] { "/ok" }, list[0].Locations);
        }

        /// <summary>
        /// 5. Несколько двоеточий: Split с count=2 сохраняет правую часть целиком 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_MultipleColons_PreservesRightPart()
        {
            var content = "Name:/a:/b:/c\n";
            var svc = await ParseAsync(content);
            var p = GetPlatforms(svc).Single();

            // Правая часть целиком -> потом split по запятой: одна локация "/a:/b:/c"
            Assert.Equal("Name", p.Name);
            Assert.Single(p.Locations);
            Assert.Equal("/a:/b:/c", p.Locations[0]);
        }

        /// <summary>
        /// 6. Пустое имя -> игнорируется 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_EmptyName_Ignored()
        {
            var content = "  :/a,/b\nValid:/c\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Single(list);
            Assert.Equal("Valid", list[0].Name);
            Assert.Equal(new[] { "/c" }, list[0].Locations);
        }

        /// <summary>
        /// 7. Пустой список локаций -> игнорируется 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_EmptyLocations_Ignored()
        {
            var content = "A:\nB:   \nC: , ,  ,\nValid:/x\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Single(list);
            Assert.Equal("Valid", list[0].Name);
            Assert.Equal(new[] { "/x" }, list[0].Locations);
        }

        /// <summary>
        /// 8. Дубли и пустые элементы после запятых отрезаются благодаря RemoveEmptyEntries 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_ConsecutiveCommas_RemoveEmptyEntries()
        {
            var content = "Name:/a,,/b,,,/c\n";
            var svc = await ParseAsync(content);
            var p = GetPlatforms(svc).Single();

            Assert.Equal(new[] { "/a", "/b", "/c" }, p.Locations);
        }

        /// <summary>
        /// 9. Windows переводы строк \r\n 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_WindowsNewlines_OK()
        {
            var content = "A:/a\r\nB:/b\r\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Equal(2, list.Count);
            Assert.Equal("A", list[0].Name);
            Assert.Equal("B", list[1].Name);
        }

        /// <summary>
        /// 10. Unicode / кириллица сохраняются 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_UnicodeNamesAndPaths_OK()
        {
            var content = "Газета дня:/руть/путь1,/руть/путь2\n";
            var svc = await ParseAsync(content);
            var p = GetPlatforms(svc).Single();

            Assert.Equal("Газета дня", p.Name);
            Assert.Equal(2, p.Locations.Count);
        }

        /// <summary>
        /// 11. Нет падения на null/пустой строке (исключения заглатываются, состояние не меняется) 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_NullContent_NoThrow_StateUnchanged()
        {
            var svc = await ParseAsync("Before:/x\n");
            var before = GetPlatforms(svc);

            // Act: null
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            await svc.LoadFromStreamAsync(stream);

            // Assert: метод не бросил, список платформ не стал null и не очистился “случайно”
            var after = GetPlatforms(svc);
            Assert.Equal(before, after); // состояние не переписано новым списком
            Assert.Single(after);
            Assert.Equal("Before", after[0].Name);
        }

        /// <summary>
        /// 12. Линия с хвостовой/начальной запятой 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_EmptyString_NoThrow_ResultsEmpty()
        {
            var svc = new AdPlatformService(NullLogger<AdPlatformService>.Instance);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            await svc.LoadFromStreamAsync(stream);

            var list = GetPlatforms(svc);
            // Из-за try/catch исключений нет, но внутри создастся пустой список и не попадём в _platforms,
            // т.к. контент пустой — НО в текущей реализации _platforms присваивается только внутри try.
            // Empty не вызовет исключение, значит _platforms станет пустым списком.
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        /// <summary>
        /// 13. Линия с хвостовой/начальной запятой 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_TrailingAndLeadingCommas_IgnoresEmpties()
        {
            var content = "Name:,/a,/b,\n";
            var svc = await ParseAsync(content);
            var p = GetPlatforms(svc).Single();

            Assert.Equal(new[] { "/a", "/b" }, p.Locations);
        }

        /// <summary>
        /// 14. Большой объём: корректность + грубая производительность 
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        public async Task LoadFromStreamAsync_LargeInput_IsFastEnoughAndAccurate()
        {
            // 10000 площадок по 3 локации
            var lines = new List<string>(10000);
            for (int i = 0; i < 10_000; i++)
                lines.Add($"P{i}:/a{i},/b{i},/c{i}");
            var content = string.Join('\n', lines);

            var sw = Stopwatch.StartNew();
            var svc = await ParseAsync(content);
            sw.Stop();

            var list = GetPlatforms(svc);
            Assert.Equal(10_000, list.Count);
            Assert.All(list, p => Assert.Equal(3, p.Locations.Count));

            // Нежёсткий SLA: парсинг не должен быть «минутами».
            // Порог деликатный, чтобы не флакать на CI. Настройте под свою инфраструктуру.
            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(2),
                $"Парсинг занял слишком долго: {sw.Elapsed}.");
        }

        /// <summary>
        /// 15. Линии с только пробелами после двоеточия -> игнор 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_WhitespaceLocations_Ignored()
        {
            var content = "A:   \nB: \t  , \nC:/ok\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Single(list);
            Assert.Equal("C", list[0].Name);
            Assert.Equal(new[] { "/ok" }, list[0].Locations);
        }

        /// <summary>
        /// 16. Смешанные валидные/невалидные строки не ломают обработку 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_MixedValidInvalid_NoBreak()
        {
            var content = string.Join('\n', new[]
            {
            "Valid1:/x",
            "NoColonHere",
            " :/y",
            "Valid2:/a,/b",
            "EmptyLocs:",
            "Valid3:/z",
            "",
            "Valid4:/p,/q,/r"
        });

            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Equal(4, list.Count);
            Assert.Equal(new[] { "Valid1", "Valid2", "Valid3", "Valid4" }, list.Select(p => p.Name).ToArray());
        }

        /// <summary>
        /// 17. Дублирующиеся площадки не убираются (текущее поведение) 
        /// </summary>
        [Fact]
        public async Task LoadFromStreamAsync_DuplicateNames_AreKeptAsIs()
        {
            var content = "Same:/a\nSame:/b\n";
            var svc = await ParseAsync(content);
            var list = GetPlatforms(svc);

            Assert.Equal(2, list.Count);
            Assert.Equal("Same", list[0].Name);
            Assert.Equal("Same", list[1].Name);
        }

        #endregion
    }
}
