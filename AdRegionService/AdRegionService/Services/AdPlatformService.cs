/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 18 ноября 2025 06:50:21
 * Version: 1.0.104
 */

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

using AdRegionService.Models;

namespace AdRegionService.Services
{
    /// <summary>
    /// Сервис взаимодействия с рекламными площадками
    /// </summary>
    public class AdPlatformService
    {
        public int PlatformCount => _platforms.Length;
        public LoadStats LastLoadStats => _lastLoadStats ?? new LoadStats();

        private ImmutableArray<AdPlatform> _platforms = ImmutableArray<AdPlatform>.Empty;
        private ImmutableDictionary<string, ImmutableHashSet<AdPlatform>> _locationIndex = ImmutableDictionary<string, ImmutableHashSet<AdPlatform>>.Empty;
        private readonly ILogger<AdPlatformService> _logger;
        private LoadStats _lastLoadStats;

        /// <summary>
        /// Для тестов
        /// </summary>
        public IReadOnlyList<AdPlatform> Platforms => _platforms.AsReadOnly();

        public AdPlatformService(ILogger<AdPlatformService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Поиск рекламных площадок в файле (in-memory)
        /// 
        /// 1–2 млн платформ и 100–500 MB файл
        /// Если файл станет >1–2 GB или платформ >10 млн, может потребоваться больше RAM т.к. все объекты AdPlatform + индексы будут в памяти
        /// </summary>
        /// <param name="stream">Поток чтения файла</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Exception</exception>
        public async Task LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var platformBuilder = ImmutableArray.CreateBuilder<AdPlatform>();
            var newIndex = new Dictionary<string, HashSet<AdPlatform>>(StringComparer.OrdinalIgnoreCase);
            int skippedLines = 0;

            try
            {
                using var reader = new StreamReader(stream);

                int lineNumber = 0;
                await foreach (var line in ReadLinesAsync(reader, cancellationToken))
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        skippedLines++;
                        continue;
                    }

                    var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2)
                    {
                        skippedLines++;
                        continue;
                    }

                    var name = parts[0];
                    var locStrings = parts[1]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (string.IsNullOrWhiteSpace(name) || locStrings.Count == 0)
                    {
                        skippedLines++;
                        continue;
                    }

                    var platform = new AdPlatform { Name = name, Locations = locStrings.ToList() };
                    platformBuilder.Add(platform);

                    foreach (var loc in locStrings)
                    {
                        if (!newIndex.TryGetValue(loc, out var set))
                        {
                            set = new HashSet<AdPlatform>();
                            newIndex[loc] = set;
                        }
                        set.Add(platform);
                    }

                    if (lineNumber % 100_000 == 0)
                        _logger.LogInformation("Обработано {LineNumber} строк...", lineNumber);
                }

                // Присваиваем только если реально есть новые платформы
                if (platformBuilder.Count > 0)
                {
                    _platforms = platformBuilder.ToImmutable();
                    _locationIndex = newIndex.ToImmutableDictionary(
                        kv => kv.Key,
                        kv => kv.Value.ToImmutableHashSet()
                    );
                    _logger.LogInformation("Загружено {Count} рекламных площадок.", _platforms.Length);
                }
                else
                {
                    _logger.LogInformation("Файл был пустой или не содержал валидных платформ. Состояние не изменено.");
                }

                _lastLoadStats = new LoadStats
                {
                    LoadedCount = platformBuilder.Count,
                    SkippedCount = skippedLines
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Загрузка рекламных площадок отменена.");
            }
            catch (OutOfMemoryException oom)
            {
                _logger.LogError(oom, "Файл слишком большой, не удалось загрузить рекламные площадки. Состояние не изменено.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке рекламных площадок. Состояние не изменено.");
            }
        }

        /// <summary>
        /// Асинхронный построчный чтение StreamReader
        /// </summary>
        private static async IAsyncEnumerable<string> ReadLinesAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return line;
            }
        }

        /// <summary>
        /// Поиск рекламных площадок в памяти (ОЗУ)
        /// 
        /// _platforms до ~1–2 млн элементов → комфортно. 
        /// Более 10 млн → стоит оптимизировать поиск (индексировать по локациям, использовать словарь или trie).
        /// </summary>
        /// <param name="location">Локация</param>
        /// <returns>Список рекламных площадок</returns>
        public IEnumerable<AdPlatform> Search(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return Array.Empty<AdPlatform>();

            location = location.Trim();

            try
            {
                var results = new HashSet<AdPlatform>();

                // Собираем все уровни локации
                var parts = location.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var prefixes = new List<string>();

                for (int i = 0; i < parts.Length; i++)
                {
                    var prefix = "/" + string.Join('/', parts.Take(i + 1));
                    prefixes.Add(prefix);
                }

                // Проверяем каждую ступень
                foreach (var prefix in prefixes)
                {
                    if (_locationIndex.TryGetValue(prefix, out var list))
                        foreach (var p in list)
                            results.Add(p);
                }

                return results;
            }
            catch (OutOfMemoryException oom)
            {
                _logger.LogError(oom, "Поиск не удался: OutOfMemoryException");
                return Array.Empty<AdPlatform>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в поиске рекламных площадок");
                return Array.Empty<AdPlatform>();
            }
        }
    }
}
