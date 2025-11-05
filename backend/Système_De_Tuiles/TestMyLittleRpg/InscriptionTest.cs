using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MyLittleRpg.Models;
using MyLittleRpg;
using System.Text.Json.Serialization;

using System.Net.Http.Json;
using FluentAssertions;
using Humanizer;
using System.Net;
using MyLittleRpg.Controllers;


namespace TestMyLittleRpg
{
    public class SimpleGameFlowTest : IClassFixture<WebApplicationFactory<MyLittleRpg.Program>>
    {
        private readonly WebApplicationFactory<MyLittleRpg.Program> _factory;
        private readonly HttpClient _client;

        public SimpleGameFlowTest(WebApplicationFactory<MyLittleRpg.Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Inscription_WithValidData_ReturnsCreated()
        {
            // Wait for initialization services to complete
            await Task.Delay(2000); // Give services time to initialize map
            var registerRequest = new RegisterRequest
            {
                Email = $"test_{Guid.NewGuid()}@test.com",
                MotDePasse = "password123",
                Pseudo = "TestHero"
            };

            var reponseDuServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

            reponseDuServeur.StatusCode.Should().Be(HttpStatusCode.Created);  
            var body = await reponseDuServeur.Content.ReadFromJsonAsync<RegisterResponse>();
            body.Should().NotBeNull();
            body.UtilisateurCréé.Should().NotBeNull();
            body.PersonnageCréé.Should().NotBeNull();
            body.UtilisateurCréé!.Id.Should().NotBe(0);
            body.PersonnageCréé!.Id.Should().NotBe(0);
        }

        [Fact]
        public async Task Inscription_WithValidData_CreatesCharacterAutomatically()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"test_{Guid.NewGuid()}@mail.com",
                Pseudo = "King",
                MotDePasse = "KingLeRoi"
            };

            var reponseDuServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            reponseDuServeur.EnsureSuccessStatusCode();

            var body = await reponseDuServeur.Content.ReadFromJsonAsync<RegisterResponse>();
            body!.PersonnageCréé.Should().NotBeNull();
            body!.PersonnageCréé!.Id.Should().BeGreaterThan(0);

            // ici on part voir le perso a bien été créé en bd
            var getPerso = await _client.GetAsync($"api/personnage/{body.UtilisateurCréé!.Id}");
            getPerso.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Inscription_WithValidData_PlacesCharacterInRandomCity()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"test_{Guid.NewGuid()}@etu.cchic.ca",
                Pseudo = "Supreme",
                MotDePasse = "KingSurpreme"
            };

            var reponseDuServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            reponseDuServeur.EnsureSuccessStatusCode();

            var body = await reponseDuServeur.Content.ReadFromJsonAsync<RegisterResponse>();
            int utilisateurId = body.UtilisateurCréé.Id;

            // recuperation du personnage
            var persoObtenu = await _client.GetAsync($"api/personnage/{utilisateurId}");
            persoObtenu.EnsureSuccessStatusCode();
            var perso = await persoObtenu.Content.ReadFromJsonAsync<GetPersonnageResponse>();
            perso.Should().NotBeNull();

            // je dois en disctuer avec Tristan pour savoir comment on gere les ville
            perso!.PositionX.Should().BeGreaterThanOrEqualTo(0);
            perso!.PositionY.Should().BeGreaterThanOrEqualTo(0);
            
        }

      
        [Fact]
        public async Task Inscription_WithExistingEmail_ReturnsConflict()
        {
            string email = $"jorel_{Guid.NewGuid()}@mail.com";
            var registerRequest = new RegisterRequest { Email = email, Pseudo = "jorel", MotDePasse = "password123" };

            var inscription1 = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            inscription1.EnsureSuccessStatusCode();

            // je reverifie qu'on ne peut pas s'inscrire avec le même email
            var inscription1_2 = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            inscription1_2.StatusCode.Should().Be(HttpStatusCode.Conflict); 
        }

        [Fact]
        public async Task Inscription_WithEmptyEmail_ReturnsBadRequest()
        {
            var registerRequest = new RegisterRequest { Email = "", Pseudo = "SuperKing", MotDePasse = "KingLeBoss" };

            var reponseServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            reponseServeur.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
        }

        [Fact]
        public async Task Inscription_WithEmptyPassword_ReturnsBadRequest()
        {
            var registerRequest = new RegisterRequest { Email = $"test_{Guid.NewGuid()}@hotmail.com", Pseudo = "PowerMan", MotDePasse = "" };

            var reponseServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            reponseServeur.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
        }

        [Fact]
        public async Task Inscription_WithEmptyPseudo_ReturnsBadRequest()
        {
            var registerRequest = new RegisterRequest { Email = $"Testx_{Guid.NewGuid()}@mail.com", Pseudo = "", MotDePasse = "King237" };

            var reponseServeur = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            reponseServeur.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
        }
    }

 
    public class RegisterRequest
    {
        public string Email { get; set; } = "";
        public string Pseudo { get; set; } = "";
        public string MotDePasse { get; set; } = "";
    }

    public class RegisterResponse
    {
        public string? message { get; set; }
        public UtilisateurDto? UtilisateurCréé { get; set; }
        public PersonnageDto? PersonnageCréé { get; set; }
    }

    public class UtilisateurDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Pseudo { get; set; } = "";
        public DateTime DateInscription { get; set; }
    }

    public class PersonnageDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PointsVie { get; set; }
        public int Force { get; set; }
    }

    // Réponse de GET api/personnage/{utilisateurId}
    public class GetPersonnageResponse
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public int UtilisateurId { get; set; }
        public string UtilisateurEmail { get; set; } = "";
        public string UtilisateurPseudo { get; set; } = "";
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public DateTime DateCreation { get; set; }

    }




}
