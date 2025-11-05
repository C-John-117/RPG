using Microsoft.EntityFrameworkCore;  
using MyLittleRpg.Data.Context;
using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public class InstanceMonsterService : IInstanceMonstreService
    {
        private readonly MyLittleRPGContext instantEnBD;
        private static readonly Random _generateurAleatoire = new Random();

        public InstanceMonsterService(MyLittleRPGContext instanceEnBase)
        {
            instantEnBD = instanceEnBase;
        }

        public async Task<InstanceMonster> ObtenirAsync(int positionX, int positionY, CancellationToken ct = default(CancellationToken))
        {
            InstanceMonster instance = await instantEnBD.InstancesMonstres.Include(i => i.Monstre)
                .FirstOrDefaultAsync(i => i.PositionX == positionX && i.PositionY == positionY, ct);

            return instance;
        }

        // Supprime tous les monstres de la base de données
        public async Task<int> SupprimerTousLesMonstresAsync(CancellationToken ct = default(CancellationToken))
        {
            var tousLesMonstres = await instantEnBD.InstancesMonstres.ToListAsync(ct);
            int count = tousLesMonstres.Count;
            
            if (count > 0)
            {
                instantEnBD.InstancesMonstres.RemoveRange(tousLesMonstres);
                await instantEnBD.SaveChangesAsync(ct);
            }
            
            return count;
        }

        public async Task<InstanceMonster> CreerAsync(int positionX, int positionY, int monstreId, int niveau, int pointsVieMax, CancellationToken ct = default(CancellationToken))
        {
            InstanceMonster existant = await ObtenirAsync(positionX, positionY, ct);
            if (existant != null) return existant;

            InstanceMonster instance = new InstanceMonster
            {
                PositionX = positionX,
                PositionY = positionY,
                MonstreId = monstreId,
                Niveau = niveau,
                PointsVieMax = pointsVieMax,
                PointsVieActuels = pointsVieMax
            };

            instantEnBD.InstancesMonstres.Add(instance);
            await instantEnBD.SaveChangesAsync(ct);

            // Recharger avec navigation (Monstre) pour les stats
            InstanceMonster recharger = await ObtenirAsync(positionX, positionY, ct);
            return recharger;
        }

        // Assure qu'il y a "cible" instances au total - génère aléatoirement sur toute la carte
        public async Task<int> AssurerNombreAsync(int cible, CancellationToken ct = default(CancellationToken))
        {
            int totalActuel = await instantEnBD.InstancesMonstres.CountAsync(ct);
            int aCreer = cible - totalActuel;
            if (aCreer <= 0) return 0;

            // Charger les positions occupées par des monstres
            var positionsOccupees = await instantEnBD.InstancesMonstres
                .Select(i => new { i.PositionX, i.PositionY })
                .ToListAsync(ct);

            var positionsOccupeesSet = new HashSet<(int X, int Y)>(
                positionsOccupees.Select(p => (p.PositionX, p.PositionY))
            );

            // Charger les villes pour éviter de placer des monstres dessus
            var positionsVilles = await instantEnBD.Tuiles
                .Where(t => t.Type == Tuile.TypeTuile.VILLE)
                .Select(t => new { t.PositionX, t.PositionY })
                .ToListAsync(ct);

            var positionsVillesSet = new HashSet<(int X, int Y)>(
                positionsVilles.Select(p => (p.PositionX, p.PositionY))
            );

            // Charger toutes les tuiles existantes pour vérifier traversabilité
            var tuilesExistantes = await instantEnBD.Tuiles
                .Select(t => new { t.PositionX, t.PositionY, t.EstTraversable })
                .ToListAsync(ct);

            var tuilesDict = new Dictionary<(int X, int Y), bool>();
            foreach (var t in tuilesExistantes)
            {
                tuilesDict[(t.PositionX, t.PositionY)] = t.EstTraversable;
            }

            int nombreMonstres = await instantEnBD.Monster.CountAsync(ct);
            if (nombreMonstres == 0) return 0;

            // Charger tous les monstres en mémoire pour éviter les requêtes répétées
            var tousLesMonstres = await instantEnBD.Monster.ToListAsync(ct);

            // Charger la liste des villes pour le calcul de niveau
            List<Tuile> listeVilles = await instantEnBD.Tuiles
                .Where(t => t.Type == Tuile.TypeTuile.VILLE)
                .ToListAsync(ct);

            // Préparer les listes pour l'insertion en lot
            var nouvellesTuiles = new List<Tuile>();
            var nouvellesMonstres = new List<InstanceMonster>();

            int tentatives = 0;
            int maxTentatives = aCreer * 10;

            while (nouvellesMonstres.Count < aCreer && tentatives < maxTentatives)
            {
                tentatives++;

                // Générer position aléatoire sur toute la carte (0-50)
                int x = _generateurAleatoire.Next(0, 51);
                int y = _generateurAleatoire.Next(0, 51);

                // Vérifier que position n'est pas occupée et n'est pas une ville
                if (positionsOccupeesSet.Contains((x, y)) || positionsVillesSet.Contains((x, y)))
                {
                    continue;
                }

                // Vérifier si tuile existe et est traversable
                if (tuilesDict.TryGetValue((x, y), out bool estTraversable))
                {
                    if (!estTraversable)
                    {
                        continue;
                    }
                }
                else
                {
                    // Tuile n'existe pas, créer une tuile traversable
                    Tuile nouvelleTuile = GenererTuileAleatoireTraversable(x, y);
                    nouvellesTuiles.Add(nouvelleTuile);
                    tuilesDict[(x, y)] = true;
                }

                // Choisir monstre aléatoire
                int indexMonstre = _generateurAleatoire.Next(nombreMonstres);
                Monster monstreBase = tousLesMonstres[indexMonstre];

                // Calculer niveau
                int niveau = CalculerNiveauDepuisVilleProche(x, y, listeVilles);
                (int force, int defense, int pointsVieMax) stats = CalculerStats(monstreBase, niveau);

                // Créer instance (sans sauvegarder)
                InstanceMonster instance = new InstanceMonster
                {
                    PositionX = x,
                    PositionY = y,
                    MonstreId = monstreBase.Id,
                    Niveau = niveau,
                    PointsVieMax = stats.pointsVieMax,
                    PointsVieActuels = stats.pointsVieMax
                };

                nouvellesMonstres.Add(instance);
                positionsOccupeesSet.Add((x, y));
            }

            // Sauvegarder tout en UNE SEULE transaction
            if (nouvellesTuiles.Count > 0)
            {
                await instantEnBD.Tuiles.AddRangeAsync(nouvellesTuiles, ct);
            }

            if (nouvellesMonstres.Count > 0)
            {
                await instantEnBD.InstancesMonstres.AddRangeAsync(nouvellesMonstres, ct);
            }

            await instantEnBD.SaveChangesAsync(ct);

            return nouvellesMonstres.Count;
        }

        // Génère tuile aléatoire traversable
        private Tuile GenererTuileAleatoireTraversable(int x, int y)
        {
            var probabilites = new[] { 40, 30, 30 };
            var types = new[] { Tuile.TypeTuile.HERBE, Tuile.TypeTuile.FORET, Tuile.TypeTuile.ROUTE };
            var images = new[] { "img/Plains.png", "img/Forest.png", "img/Road.png" };

            int totalPoids = probabilites.Sum();
            int nombreAleatoire = _generateurAleatoire.Next(0, totalPoids);
            int poidsActuel = 0;
            int index = 0;

            for (int i = 0; i < probabilites.Length; i++)
            {
                poidsActuel += probabilites[i];
                if (nombreAleatoire < poidsActuel)
                {
                    index = i;
                    break;
                }
            }

            return new Tuile
            {
                PositionX = x,
                PositionY = y,
                Type = types[index],
                EstTraversable = true,
                ImageUrl = images[index]
            };
        }

        public int CalculerNiveauDepuisVilleProche(int x, int y, IEnumerable<Tuile> listeVilles)
        {
            bool aucuneVille = true;
            int distanceMinimale = int.MaxValue;

            foreach (Tuile ville in listeVilles)
            {
                aucuneVille = false;
                int distance = Math.Abs(ville.PositionX - x) + Math.Abs(ville.PositionY - y);
                if (distance < distanceMinimale) distanceMinimale = distance;
            }

            if (aucuneVille) return 1;

            int niveau = 1 + distanceMinimale;
            if (niveau < 1) niveau = 1;
            if (niveau > 20) niveau = 20;
            return niveau;
        }

        public (int force, int defense, int pointsVieMax) CalculerStats(Monster monstreBase, int niveau)
        {
            // Formule selon l'énoncé: StatBase + Niveau
            int force = monstreBase.ForceBase + niveau;
            int defense = monstreBase.DefenseBase + niveau;
            int pointsVieMax = monstreBase.PointsVieBase + niveau;
            return (force, defense, pointsVieMax);
        }
    }
}
