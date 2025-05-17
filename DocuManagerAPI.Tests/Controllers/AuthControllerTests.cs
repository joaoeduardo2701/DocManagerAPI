using DocuManagerAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace DocuManagerAPI.Tests
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // importante para ver os cookies corretamente
            });
        }

        [Fact]
        public async Task Login_DeveRetornarCookie_QuandoCredenciaisForemCorretas()
        {
            // Arrange
            var loginData = new UserDtocs
            {
                Email = "admin@docu.com",
                Senha = "123456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verifica se o cookie JWT foi retornado
            Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");
            var setCookie = response.Headers.GetValues("Set-Cookie");
            Assert.Contains(setCookie, c => c.StartsWith("DocuToken"));
        }
    }
}
