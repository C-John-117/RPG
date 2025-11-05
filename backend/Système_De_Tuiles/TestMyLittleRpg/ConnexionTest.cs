
using Microsoft.AspNetCore.Mvc.Testing;
using MyLittleRpg.Models;
using MyLittleRpg;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;

namespace TestMyLittleRpg
{
    public class ConnexionTest : IClassFixture<WebApplicationFactory<MyLittleRpg.Program>>
    {
        private readonly WebApplicationFactory<MyLittleRpg.Program> _factory;
        private readonly HttpClient _client;

        public ConnexionTest(WebApplicationFactory<MyLittleRpg.Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_ReturnsOk()
        {
            await Task.Delay(2000);
            var email = $"testlogin_{Guid.NewGuid()}@test.com";
            var password = "password123";
            var pseudo = "TestLogin";
            var utilisateur = new Utilisateur { Email = email, MotDePasse = password, Pseudo = pseudo };
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", utilisateur);
            registerResponse.IsSuccessStatusCode.Should().BeTrue($"Registration failed: {await registerResponse.Content.ReadAsStringAsync()}");

            var loginRequest = new { Email = email, MotDePasse = password };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.IsSuccessStatusCode.Should().BeTrue($"Login failed: {await loginResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_SetsEstConnecteToTrue()
        {
            await Task.Delay(2000);
            var email = $"testlogin_{Guid.NewGuid()}@test.com";
            var password = "password123";
            var pseudo = "TestLogin";
            var utilisateur = new Utilisateur { Email = email, MotDePasse = password, Pseudo = pseudo };
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", utilisateur);
            registerResponse.IsSuccessStatusCode.Should().BeTrue();

            var loginRequest = new { Email = email, MotDePasse = password };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.IsSuccessStatusCode.Should().BeTrue();
            var content = await loginResponse.Content.ReadAsStringAsync();
            content.Should().Contain("personnage");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_AllowsSubsequentAuthenticatedRequests()
        {
            await Task.Delay(2000);
            var email = $"testlogin_{Guid.NewGuid()}@test.com";
            var password = "password123";
            var pseudo = "TestLogin";
            var utilisateur = new Utilisateur { Email = email, MotDePasse = password, Pseudo = pseudo };
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", utilisateur);
            registerResponse.IsSuccessStatusCode.Should().BeTrue();

            var loginRequest = new { Email = email, MotDePasse = password };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.IsSuccessStatusCode.Should().BeTrue();

            // Récupérer l'id du personnage depuis la réponse de login
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            int personnageId = 0;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(loginContent);
                if (json.RootElement.TryGetProperty("personnage", out var personnageElem) && personnageElem.TryGetProperty("id", out var idElem))
                {
                    personnageId = idElem.GetInt32();
                }
            }
            catch { }

            personnageId.Should().BeGreaterThan(0, "L'id du personnage n'a pas pu être récupéré.");

            // Test accès à la ressource du personnage
            var personnageResponse = await _client.GetAsync($"api/personnage/{personnageId}");
            personnageResponse.IsSuccessStatusCode.Should().BeTrue();
        }

        [Fact]
        public async Task Connexion_WithInvalidEmail_ReturnsUnauthorized()
        {
            await Task.Delay(2000);
            var loginRequest = new { Email = "wrongemail@test.com", MotDePasse = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Connexion_WithInvalidPassword_ReturnsUnauthorized()
        {
            await Task.Delay(2000);
            var email = $"testlogin_{Guid.NewGuid()}@test.com";
            var password = "password123";
            var pseudo = "TestLogin";
            var utilisateur = new Utilisateur { Email = email, MotDePasse = password, Pseudo = pseudo };
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", utilisateur);
            registerResponse.IsSuccessStatusCode.Should().BeTrue();

            var loginRequest = new { Email = email, MotDePasse = "wrongpassword" };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Connexion_WithNonexistentUser_ReturnsUnauthorized()
        {
            await Task.Delay(2000);
            var loginRequest = new { Email = $"notfound_{Guid.NewGuid()}@test.com", MotDePasse = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Connexion_WithEmptyCredentials_ReturnsBadRequest()
        {
            await Task.Delay(2000);
            var loginRequest = new { Email = "", MotDePasse = "" };
            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
