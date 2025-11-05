using Microsoft.AspNetCore.Mvc;
using MyLittleRpg.Data.Context;
using MyLittleRpg.Models;
using MyLittleRpg.DTO;
using MyLittleRpg.Services;
using Microsoft.EntityFrameworkCore;

namespace MyLittleRpg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonnageController : ControllerBase
    {
        private readonly MyLittleRPGContext _context;
        private readonly ICombatService _combatService;
        private readonly IProgressionService _progressionService;
        private readonly IInstanceMonstreService _instanceMonstreService;
        private static int _monstresVaincusDepuisRegeneration = 0;
        private static readonly object _lock = new object();

        public PersonnageController( MyLittleRPGContext context, ICombatService combatService, IProgressionService progressionService,
            IInstanceMonstreService instanceMonstreService)
        {
            _context = context;
            _combatService = combatService;
            _progressionService = progressionService;
            _instanceMonstreService = instanceMonstreService;
        }

        // GET: api/personnage/{utilisateurId}
        [HttpGet("{utilisateurId}")]
        public async Task<ActionResult> GetPersonnage(int utilisateurId)
        {
            try
            {
                // Récupérer le personnage associé à l'utilisateur
                var personnage = await _context.Personnages
                    .Include(p => p.Utilisateur)
                    .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId);

                if (personnage == null)
                {
                    return NotFound("Aucun personnage trouvé pour cet utilisateur.");
                }

                // Retourner les informations du personnage sans le mot de passe de l'utilisateur
                var result = new
                {
                    personnage.Id,
                    personnage.Nom,
                    personnage.Niveau,
                    personnage.Experience,
                    personnage.PointsVie,
                    personnage.PointsVieMax,
                    personnage.Force,
                    personnage.Defense,
                    personnage.PositionX,
                    personnage.PositionY,
                    personnage.DateCreation,
                    Utilisateur = new
                    {
                        personnage.Utilisateur.Id,
                        personnage.Utilisateur.Email,
                        personnage.Utilisateur.Pseudo
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur: {ex.Message}");
            }
        }

        // POST: api/personnage/move
        [HttpPost("move")]
        public async Task<ActionResult<ResultatDeplacementDto>> DeplacerPersonnage(MoveRequest request)
        {
            try
            {
                // Récupérer le personnage
                var personnage = await _context.Personnages
                    .FirstOrDefaultAsync(p => p.UtilisateurId == request.UtilisateurId);

                if (personnage == null)
                {
                    return NotFound("Personnage non trouvé.");
                }

                // VALIDATION 1: Vérifier la distance (1 case adjacente seulement)
                var deltaX = Math.Abs(request.NouvellePositionX - personnage.PositionX);
                var deltaY = Math.Abs(request.NouvellePositionY - personnage.PositionY);

                if (deltaX > 1 || deltaY > 1 || (deltaX == 0 && deltaY == 0))
                {
                    return BadRequest("Déplacement invalide. Vous ne pouvez vous déplacer que d'une case adjacente.");
                }

                // VALIDATION 2: Vérifier que la tuile de destination existe et est traversable
                var tuileDestination = await _context.Tuiles
                    .FirstOrDefaultAsync(t => t.PositionX == request.NouvellePositionX &&
                                             t.PositionY == request.NouvellePositionY);

                if (tuileDestination == null)
                {
                    // Créer la tuile si elle n'existe pas (comme dans TuilesController)
                    tuileDestination = GenererTuileAleatoire(request.NouvellePositionX, request.NouvellePositionY);
                    _context.Tuiles.Add(tuileDestination);
                    await _context.SaveChangesAsync();
                }

                // on regarde sil ya une instance de monstre sur la tuile
                InstanceMonster? instanceSurTuile = await _context.InstancesMonstres.Include(
                                i => i.Monstre).FirstOrDefaultAsync(i => i.PositionX == tuileDestination.PositionX &&
                                                                     i.PositionY == tuileDestination.PositionY);
                if (instanceSurTuile == null)
                {
                    if (!tuileDestination.EstTraversable)
                    {
                        return BadRequest("Désolé cette tuile n'est pas traversable.");
                    }
                    // Mettre à jour la position
                    personnage.PositionX = tuileDestination.PositionX;
                    personnage.PositionY = tuileDestination.PositionY;


                    if (tuileDestination.Type == Tuile.TypeTuile.VILLE)
                    {
                        personnage.domicileX = tuileDestination.PositionX;
                        personnage.domicileY = tuileDestination.PositionY;
                    }

                    // LOGIQUE D'EXPLORATION: Explorer automatiquement les 8 tuiles adjacentes
                    await ExplorerTuilesAdjacentes(personnage.PositionX, personnage.PositionY);

                    // Sauvegarder les changements
                    await _context.SaveChangesAsync();

                    ResultatDeplacementDto resultatDeplacement = new ResultatDeplacementDto
                    {
                        resultat = ResultatDeplacement.Deplacer,
                        NouvellePosition = new PositionDto { X = personnage.PositionX, Y = personnage.PositionY },
                        TuileCourante = new TuileDto
                        {
                            X = tuileDestination.PositionX,
                            Y = tuileDestination.PositionY,
                            Type = tuileDestination.Type.ToString(),
                            EstTraversable = tuileDestination.EstTraversable,
                            ImageUrl = tuileDestination.ImageUrl
                        },
                        Joueur = new PersonnageDto
                        {
                            Id = personnage.Id,
                            Nom = personnage.Nom,
                            Niveau = personnage.Niveau,
                            Experience = personnage.Experience,
                            PointsVie = personnage.PointsVie,
                            PointsVieMax = personnage.PointsVieMax,
                            Force = personnage.Force,
                            Defense = personnage.Defense,
                            PositionX = personnage.PositionX,
                            PositionY = personnage.PositionY
                        }
                    };

                    return Ok(resultatDeplacement);
                }
                // SI un monstre est présent sur la tuile

                if (instanceSurTuile.Monstre == null)
                {
                    // Sécurité : si la navigation n'a pas chargé, on recharge 
                    instanceSurTuile.Monstre = await _context.Monster.FirstOrDefaultAsync(m => m.Id == instanceSurTuile.MonstreId);
                }

                ResultatCombat resultatCombat = _combatService.Resoudre(personnage, instanceSurTuile, instanceSurTuile.Monstre);

                // Appliquer PV
                instanceSurTuile.PointsVieActuels = resultatCombat.HpMonstreApres;
                personnage.PointsVie = resultatCombat.HpJoueurApres;

                CombatRapportDto rapport = new CombatRapportDto
                {
                    DegatsAuMonstre = resultatCombat.DegatsAuMonstre,
                    DegatsAuJoueur = resultatCombat.DegatsAuJoueur,
                    HpMonstreApres = resultatCombat.HpMonstreApres,
                    HpJoueurApres = resultatCombat.HpJoueurApres,
                    JoueurVainqueur = resultatCombat.JoueurVainqueur,
                    MonstreVainqueur = resultatCombat.MonstreVainqueur
                };

                ResultatDeplacementDto resultatDto = new ResultatDeplacementDto();
                resultatDto.Combat = rapport;

                if (resultatCombat.JoueurVainqueur)
                {
                    int experienceGagnee = _progressionService.XpGagnee(instanceSurTuile.Monstre, instanceSurTuile.Niveau);
                    personnage.Experience = personnage.Experience + experienceGagnee;

                    // Suppression du monstre vaincu
                    _context.InstancesMonstres.Remove(instanceSurTuile);

                    // le joueur se déplace sur la tuile doù il yavait le monstre
                    personnage.PositionX = tuileDestination.PositionX;
                    personnage.PositionY = tuileDestination.PositionY;

                    // Si c'est une ville, mémoriser le domicile
                    bool niveauAugmente = _progressionService.AppliquerMonteesDeNiveau(personnage);
                    await _context.SaveChangesAsync();

                    resultatDto.resultat = ResultatDeplacement.Deplacer;
                    resultatDto.NouvellePosition = new PositionDto { X = personnage.PositionX, Y = personnage.PositionY };
                    resultatDto.Joueur = new PersonnageDto
                    {
                        Id = personnage.Id,
                        Nom = personnage.Nom,
                        Niveau = personnage.Niveau,
                        Experience = personnage.Experience,
                        PointsVie = personnage.PointsVie,
                        PointsVieMax = personnage.PointsVieMax,
                        Force = personnage.Force,
                        Defense = personnage.Defense,
                        PositionX = personnage.PositionX,
                        PositionY = personnage.PositionY
                    };
                    resultatDto.TuileCourante = new TuileDto
                    {
                        X = tuileDestination.PositionX,
                        Y = tuileDestination.PositionY,
                        Type = tuileDestination.Type.ToString(),
                        EstTraversable = tuileDestination.EstTraversable,
                        ImageUrl = tuileDestination.ImageUrl
                    };

                    // Régénération automatique après 10 monstres vaincus
                    lock (_lock)
                    {
                        _monstresVaincusDepuisRegeneration++;
                        if (_monstresVaincusDepuisRegeneration >= 10)
                        {
                            _monstresVaincusDepuisRegeneration = 0;
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    int crees = await _instanceMonstreService.AssurerNombreAsync(300);
                                    Console.WriteLine($"🔄 Régénération : {crees} monstres créés");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"❌ Erreur régénération : {ex.Message}");
                                }
                            });
                        }
                    }

                    return Ok(resultatDto);
                }


                // D E F A I T E    D U     J O U E U R  
                if (resultatCombat.MonstreVainqueur)
                {
                    // Téléportation au domicile et restauration des points de vie
                    personnage.PositionX = personnage.domicileX;
                    personnage.PositionY = personnage.domicileY;
                    personnage.PointsVie = personnage.PointsVieMax;

                    await _context.SaveChangesAsync();

                    resultatDto.resultat = ResultatDeplacement.TeleporterAlaMaison;
                    resultatDto.NouvellePosition = new PositionDto { X = personnage.domicileX, Y = personnage.domicileY };
                    resultatDto.Joueur = new PersonnageDto
                    {
                        Id = personnage.Id,
                        Nom = personnage.Nom,
                        Niveau = personnage.Niveau,
                        Experience = personnage.Experience,
                        PointsVie = personnage.PointsVie,
                        PointsVieMax = personnage.PointsVieMax,
                        Force = personnage.Force,
                        Defense = personnage.Defense,
                        PositionX = personnage.PositionX,
                        PositionY = personnage.PositionY
                    };
                    resultatDto.TuileCourante = null;

                    return Ok(resultatDto);
                }

                // Cas où les deux sont encore en vie 
                await _context.SaveChangesAsync();

                resultatDto.resultat = ResultatDeplacement.BloquerMontrer;
                resultatDto.NouvellePosition = new PositionDto { X = personnage.PositionX, Y = personnage.PositionY };
                resultatDto.Joueur = new PersonnageDto
                {
                    Id = personnage.Id,
                    Nom = personnage.Nom,
                    Niveau = personnage.Niveau,
                    Experience = personnage.Experience,
                    PointsVie = personnage.PointsVie,
                    PointsVieMax = personnage.PointsVieMax,
                    Force = personnage.Force,
                    Defense = personnage.Defense,
                    PositionX = personnage.PositionX,
                    PositionY = personnage.PositionY
                };
                resultatDto.TuileCourante = null;

                return Ok(resultatDto);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Erreur: " + exception.Message);
            }

        }

        // GET: api/personnage/exploration/{utilisateurId}
        [HttpGet("exploration/{utilisateurId}")]
        public async Task<ActionResult> GetTuilesVisibles(int utilisateurId)
        {
            try
            {
                // Récupérer la position du personnage
                var personnage = await _context.Personnages
                    .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId);

                if (personnage == null)
                {
                    return NotFound("Personnage non trouvé.");
                }

                // Récupérer les tuiles dans un rayon de vision 5x5 centré sur le personnage
                var rayonVision = 2; // 5x5 = rayon de 2 cases
                var tuilesVisibles = await _context.Tuiles
                    .Where(t => t.PositionX >= personnage.PositionX - rayonVision &&
                               t.PositionX <= personnage.PositionX + rayonVision &&
                               t.PositionY >= personnage.PositionY - rayonVision &&
                               t.PositionY <= personnage.PositionY + rayonVision)
                    .ToListAsync();

                return Ok(new
                {
                    positionPersonnage = new { X = personnage.PositionX, Y = personnage.PositionY },
                    rayonVision = rayonVision,
                    tuilesVisibles = tuilesVisibles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur: {ex.Message}");
            }
        }

        // MÉTHODE PRIVÉE: Explorer les tuiles adjacentes (optimisée)
        private async Task ExplorerTuilesAdjacentes(int centreX, int centreY)
        {
            // Directions pour les 8 cases adjacentes
            var directions = new (int dx, int dy)[]
            {
                (-1, -1), (-1, 0), (-1, 1),
                (0, -1),           (0, 1),
                (1, -1),  (1, 0),  (1, 1)
            };

            var nouvellesTouiles = new List<Tuile>();

            foreach (var (dx, dy) in directions)
            {
                var x = centreX + dx;
                var y = centreY + dy;

                // Vérifier si la tuile existe déjà
                var tuileExistante = await _context.Tuiles
                    .FirstOrDefaultAsync(t => t.PositionX == x && t.PositionY == y);

                if (tuileExistante == null)
                {
                    // Créer une nouvelle tuile explorée
                    var nouvelleTuile = GenererTuileAleatoire(x, y);
                    nouvellesTouiles.Add(nouvelleTuile);
                }
            }

            // Ajouter toutes les nouvelles tuiles en une seule transaction (OPTIMISATION)
            if (nouvellesTouiles.Any())
            {
                _context.Tuiles.AddRange(nouvellesTouiles);
                await _context.SaveChangesAsync();
            }
        }

        // MÉTHODE PRIVÉE: Génération aléatoire de tuiles (copié de TuilesController)
        private Tuile GenererTuileAleatoire(int x, int y)
        {
            var random = new Random();
            var probabilites = new Dictionary<Tuile.TypeTuile, int>
            {
                { Tuile.TypeTuile.HERBE, 20 },
                { Tuile.TypeTuile.EAU, 10 },
                { Tuile.TypeTuile.MONTAGNE, 15 },
                { Tuile.TypeTuile.FORET, 15 },
                { Tuile.TypeTuile.VILLE, 5 },
                { Tuile.TypeTuile.ROUTE, 35 }
            };

            var totalPoids = probabilites.Values.Sum();
            var nombreAleatoire = random.Next(0, totalPoids);
            var poidsActuel = 0;

            foreach (var kvp in probabilites)
            {
                poidsActuel += kvp.Value;
                if (nombreAleatoire < poidsActuel)
                {
                    return new Tuile
                    {
                        PositionX = x,
                        PositionY = y,
                        Type = kvp.Key,
                        EstTraversable = EstTraversable(kvp.Key),
                        ImageUrl = GetImageUrl(kvp.Key)
                    };
                }
            }

            // Par défaut, retourner une tuile d'herbe
            return new Tuile
            {
                PositionX = x,
                PositionY = y,
                Type = Tuile.TypeTuile.HERBE,
                EstTraversable = true,
                ImageUrl = "img/Plains.png"
            };
        }

        private bool EstTraversable(Tuile.TypeTuile type)
        {
            return type switch
            {
                Tuile.TypeTuile.HERBE => true,
                Tuile.TypeTuile.FORET => true,
                Tuile.TypeTuile.VILLE => true,
                Tuile.TypeTuile.ROUTE => true,
                Tuile.TypeTuile.EAU => false,
                Tuile.TypeTuile.MONTAGNE => false,
                _ => true
            };
        }

        private string GetImageUrl(Tuile.TypeTuile type)
        {
            return type switch
            {
                Tuile.TypeTuile.HERBE => "img/Plains.png",
                Tuile.TypeTuile.EAU => "img/River.png",
                Tuile.TypeTuile.MONTAGNE => "img/Mountain.png",
                Tuile.TypeTuile.FORET => "img/Forest.png",
                Tuile.TypeTuile.VILLE => "img/Town.png",
                Tuile.TypeTuile.ROUTE => "img/Road.png",
                _ => "img/Plains.png"
            };
        }
    }

    // DTO pour le déplacement
    public class MoveRequest
    {
        public int UtilisateurId { get; set; }
        public int NouvellePositionX { get; set; }
        public int NouvellePositionY { get; set; }
    }
}