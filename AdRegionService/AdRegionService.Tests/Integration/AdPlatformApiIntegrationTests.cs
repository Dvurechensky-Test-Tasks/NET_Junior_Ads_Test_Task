/*
 * Author: Nikolay Dvurechensky
 * Site: https://sites.google.com/view/dvurechensky
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 30 августа 2025 08:33:16
 * Version: 1.0.23
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using AdRegionService.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace AdRegionService.Tests.Integration
{
    // Собственная фабрика, которая ставит контент на путь проекта
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Указываем путь к проекту, где лежит Program.cs
            var projectDir = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\AdRegionService");
            builder.UseContentRoot(projectDir);
            // Любые дополнительные конфигурации можно сюда
        }
    }

    public class AdPlatformApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AdPlatformApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task LoadEndpoint_WithValidFile_ReturnsOkAndStats()
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("TestPlatform:/loc1,/loc2"));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            content.Add(fileContent, "file", "platforms.txt");

            var response = await _client.PostAsync("/api/load", content);
            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            Assert.Contains("Data loaded", resultJson);
            Assert.Contains("loaded", resultJson);
            Assert.Contains("skipped", resultJson);
        }

        [Fact]
        public async Task SearchEndpoint_ReturnsPlatformsForLocation()
        {
            // Загружаем данные
            var loadContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("TestPlatform:/loc1,/loc2"));
            loadContent.Add(fileContent, "file", "platforms.txt");
            await _client.PostAsync("/api/load", loadContent);

            // Выполняем поиск
            var searchResponse = await _client.GetAsync("/api/search?location=/loc1");
            searchResponse.EnsureSuccessStatusCode();

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            Assert.Contains("TestPlatform", searchJson);
        }
    }
}
