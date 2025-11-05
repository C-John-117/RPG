using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRpg.Data.Context;
using MyLittleRpg.Models;
using NuGet.Common;
using System.ComponentModel.DataAnnotations;

namespace MyLittleRpg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MyLittleRPGContext _context;

        public AuthController(MyLittleRPGContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            try
            {
                // Vérifier si l'email existe déjà
                var existingUser = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == request.Email);
                
                if (existingUser != null)
                {
                    return Conflict("Un utilisateur avec cet email existe déjà.");
                    //return BadRequest("Un utilisateur avec cet email existe déjà.");
                }

                // Créer l'utilisateur (mot de passe en clair pour simplicité)
                var passwordHasher = new PasswordHasher<Utilisateur>();
                var utilisateur = new Utilisateur
                {
                    Email = request.Email,
                    Pseudo = request.Pseudo,
                    MotDePasse = passwordHasher.HashPassword(null, request.MotDePasse),
                    DateInscription = DateTime.Now
                };

                _context.Utilisateurs.Add(utilisateur);
                await _context.SaveChangesAsync();

                // Créer un personnage par défaut
                Personnage personnage = new Personnage
                {
                    Nom = request.Pseudo + " Hero",
                    Niveau = 1,
                    Experience = 0,
                    PointsVie = 100,
                    PointsVieMax = 100,
                    Force = 20,
                    Defense = 12,
                    PositionX = 10,
                    PositionY = 10,
                    UtilisateurId = utilisateur.Id,
                    DateCreation = DateTime.Now
                };

                _context.Personnages.Add(personnage);
                await _context.SaveChangesAsync();


                Utilisateur nUtilisateur = new Utilisateur
                {
                   Id =  utilisateur.Id,
                   Email = utilisateur.Email,
                   Pseudo = utilisateur.Pseudo,
                   DateInscription=  utilisateur.DateInscription
                };

                Personnage nPersonnage = new Personnage
                {
                    Id = personnage.Id,
                    Nom = personnage.Nom,
                    PositionX = personnage.PositionX,
                    PositionY = personnage.PositionY,
                    PointsVie =  personnage.PointsVie,
                    Force = personnage.Force
                };

                return Created($"/api/personnage/{utilisateur.Id}", new
                {
                    message = "Inscription réussie",
                    UtilisateurCréé = nUtilisateur,
                    PersonnageCréé = nPersonnage
                });
                //return Ok(new { 
                //    message = "Inscription réussie",
                //    nUtilisateur,
                //    nPersonnage
                //});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur: {ex.Message}");
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest request)
        {
            try
            {
                // Vérification des identifiants vides
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.MotDePasse))
                {
                    return BadRequest("Email et mot de passe sont requis.");
                }

                // Trouver l'utilisateur avec mot de passe simple
                var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (utilisateur == null)
                {
                    return Unauthorized("Email ou mot de passe incorrect.");
                }

                var passwordHasher = new PasswordHasher<Utilisateur>();
                var result = passwordHasher.VerifyHashedPassword(null, utilisateur.MotDePasse, request.MotDePasse);

                if (result != PasswordVerificationResult.Success)
                {
                    return Unauthorized("Email ou mot de passe incorrect.");
                }

                // Récupérer le personnage associé
                var personnage = await _context.Personnages
                    .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateur.Id);

                return Ok(new
                {
                    personnage = new
                    {
                        id = personnage.Id,
                        utilisateurId = personnage.UtilisateurId,
                        nom = personnage.Nom,
                        niveau = personnage.Niveau,
                        experience = personnage.Experience,
                        force = personnage.Force,
                        defense = personnage.Defense,
                        pointsVie = personnage.PointsVie,
                        pointsVieMax = personnage.PointsVieMax,
                        positionX = personnage.PositionX,
                        positionY = personnage.PositionY
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur: {ex.Message}");
            }
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public ActionResult Logout()
        {
            return Ok(new { message = "Déconnexion réussie" });
        }
    }

    // DTOs simples

    public class RegisterRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide, veuillez le corriger s'il vous plait.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le pseudo est obligatoire.")]
        public string Pseudo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La longueur du mot de passe n'est pas la bonne"), MinLength(6)]
        public string MotDePasse { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide, veuillez le corriger s'il vous plait.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La longueur du mot de passe n'est pas la bonne"), MinLength(6)]
        public string MotDePasse { get; set; } = string.Empty;
    }
}