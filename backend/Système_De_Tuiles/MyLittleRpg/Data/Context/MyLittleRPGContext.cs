using Microsoft.EntityFrameworkCore;
using MyLittleRpg.Models;

namespace MyLittleRpg.Data.Context
{
    public class MyLittleRPGContext : DbContext
    {
        public DbSet<Monster> Monster { get; set; }
        public DbSet<Tuile> Tuiles { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Personnage> Personnages { get; set; }
        public DbSet<InstanceMonster> InstancesMonstres { get; set; }


        public MyLittleRPGContext(DbContextOptions<MyLittleRPGContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chaque tuile peut contenir au plus un monstre. Suppression de la tuile entraine supprime aussi l’instance.
            modelBuilder.Entity<InstanceMonster>()
                .HasOne<Tuile>()
                .WithOne()
                .HasForeignKey<InstanceMonster>(im => new { im.PositionX, im.PositionY })
                .OnDelete(DeleteBehavior.Cascade);

            // Un type de monstre peut avoir plusieurs instances et on rend mpossible de supprimer un monstre encore utilisé
            modelBuilder.Entity<InstanceMonster>()
                .HasOne(im => im.Monstre)
                .WithMany()
                .HasForeignKey(im => im.MonstreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chaque utilisateur a un personnage unique, et chaque personnage doit être lié à un utilisateur.
            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Personnage)
                .WithOne(p => p.Utilisateur)
                .HasForeignKey<Personnage>(p => p.UtilisateurId)
                .IsRequired();
        }


    }
}
