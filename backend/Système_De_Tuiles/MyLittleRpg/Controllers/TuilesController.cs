using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRpg.Data.Context;
using MyLittleRpg.Models;
using MyLittleRpg.DTO;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace MyLittleRpg.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuilesController : ControllerBase
    {
        private readonly MyLittleRPGContext _context;

        public TuilesController(MyLittleRPGContext context)
        {
            _context = context;
        }

        // GET: api/Tuiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tuile>>> GetTuiles()
        {
            return await _context.Tuiles.ToListAsync();
        }

        // GET: api/Tuiles/5
        [HttpGet("{abcisse}/{ordonnee}")]
        public async Task<ActionResult<Tuile>> GetTuile(int abcisse, int ordonnee)
        {

            if (abcisse < 0 || abcisse > 50 || ordonnee < 0 || ordonnee > 50)
            {
                return BadRequest("Les coordonnées doivent être comprises entre 0 et 50.");
            }
            Tuile tuile = await _context.Tuiles.FindAsync(abcisse, ordonnee);

            if (tuile == null)
            {

                tuile = CreationDeTuile(abcisse, ordonnee);
                _context.Tuiles.Add(tuile);
                await _context.SaveChangesAsync();
            }
            // jai modifié pour que ca correpsond avec le front, il yvait un soucis
            return Ok(new
            {
                positionX = tuile.PositionX,
                positionY = tuile.PositionY,
                type = (int)tuile.Type,
                estTraversable = tuile.EstTraversable,
                imageUrl = tuile.ImageUrl
            });
            //return tuile;
        }

        // GET: api/Tuiles/details/{x}/{y} - Retourne la tuile avec les détails du monstre
        [HttpGet("details/{x}/{y}")]
        public async Task<ActionResult<TuileDto>> GetTuileDetails(int x, int y)
        {
            if (x < 0 || x > 50 || y < 0 || y > 50)
            {
                return BadRequest("Les coordonnées doivent être comprises entre 0 et 50.");
            }

            // Chercher ou créer la tuile
            Tuile? tuile = await _context.Tuiles.FindAsync(x, y);
            if (tuile == null)
            {
                tuile = CreationDeTuile(x, y);
                _context.Tuiles.Add(tuile);
                await _context.SaveChangesAsync();
            }

            // Créer le DTO de base
            var tuileDto = new TuileDto
            {
                X = tuile.PositionX,
                Y = tuile.PositionY,
                Type = tuile.Type.ToString(),
                EstTraversable = tuile.EstTraversable,
                ImageUrl = tuile.ImageUrl,
                Monstre = null
            };

            // Chercher s'il y a un monstre sur cette tuile
            var instanceMonstre = await _context.InstancesMonstres
                .Include(im => im.Monstre)
                .FirstOrDefaultAsync(im => im.PositionX == x && im.PositionY == y);

            if (instanceMonstre != null && instanceMonstre.Monstre != null)
            {
                int niveau = instanceMonstre.Niveau;
                int pvMax = (instanceMonstre.Monstre.PointsVieBase) + niveau;
                int force = (instanceMonstre.Monstre.ForceBase) + niveau;
                int defense = (instanceMonstre.Monstre.DefenseBase) + niveau;

                tuileDto.Monstre = new InstanceMonstreDto
                {
                    X = instanceMonstre.PositionX,
                    Y = instanceMonstre.PositionY,
                    MonstreId = instanceMonstre.MonstreId,
                    Nom = instanceMonstre.Monstre.Nom,
                    SpriteURL = instanceMonstre.Monstre.SpriteURL,
                    Type1 = instanceMonstre.Monstre.Type1,
                    Type2 = instanceMonstre.Monstre.Type2,
                    Niveau = niveau,
                    PointsVieActuels = instanceMonstre.PointsVieActuels,
                    PointsVieMax = pvMax,
                    Force = force,
                    Defense = defense
                };
            }

            return Ok(tuileDto);
        }

        // POST: api/Tuiles/batch






        // Métthode pour créer une tuile aléatoire
        private Tuile CreationDeTuile(int abcisse, int ordonnee)
        {
            Tuile tuile = new Tuile();
            tuile.PositionX = abcisse;
            tuile.PositionY = ordonnee;
            Tuile.TypeTuile typeDeTuile;
            bool estTraversable;
            string image;
            Random random = new Random();
            int probabilité = random.Next(100);

            if (probabilité < 20)
            {
                typeDeTuile = Tuile.TypeTuile.HERBE;
                estTraversable = true;
                image = "img/Plains.png";
            }
            else if (probabilité < 30)
            {
                typeDeTuile = Tuile.TypeTuile.EAU;
                estTraversable = false;
                image = "img/River.png";
            }
            else if (probabilité < 45)
            {
                typeDeTuile = Tuile.TypeTuile.MONTAGNE;
                estTraversable = false;
                image = "img/Mountain.png";
            }
            else if (probabilité < 60)
            {
                typeDeTuile = Tuile.TypeTuile.FORET;
                estTraversable = true;
                image = "img/Forest.png";
            }
            else if (probabilité < 65)
            {
                typeDeTuile = Tuile.TypeTuile.VILLE;
                estTraversable = true;
                image = "img/Town.png";
            }
            else
            {
                typeDeTuile = Tuile.TypeTuile.ROUTE;
                estTraversable = true;
                image = "img/Road.png";
            }

            tuile.Type = typeDeTuile;
            tuile.EstTraversable = estTraversable;
            tuile.ImageUrl = image;

            return tuile;
        }

        // PUT: api/Tuiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTuile(int id, Tuile tuile)
        {
            if (id != tuile.PositionX)
            {
                return BadRequest();
            }

            _context.Entry(tuile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TuileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tuiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tuile>> PostTuile(Tuile tuile)
        {
            _context.Tuiles.Add(tuile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TuileExists(tuile.PositionX))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTuile", new { id = tuile.PositionX }, tuile);
        }

        // DELETE: api/Tuiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTuile(int id)
        {
            var tuile = await _context.Tuiles.FindAsync(id);
            if (tuile == null)
            {
                return NotFound();
            }

            _context.Tuiles.Remove(tuile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TuileExists(int id)
        {
            return _context.Tuiles.Any(e => e.PositionX == id);
        }
    }
}
