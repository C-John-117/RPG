
using MyLittleRpg.Data.Context;
using Microsoft.EntityFrameworkCore;
using MyLittleRpg.Services;

namespace MyLittleRpg
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policybuilder =>
                {
                    // Configuration CORS pour développement - autorise toutes les origines
                    policybuilder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                });
            });

            builder.Services.AddDbContext<MyLittleRPGContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Default");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
            );

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON pour accepter camelCase du frontend et le convertir en PascalCase
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });


            /* AddScoped indique que l�instance du service sera cr��e une fois par requ�te HTTP
            ICombatService, CombatService signifie que lorsque ton code demande une d�pendance du type ICombatService, 
            le conteneur d�injection de d�pendances fournira une instance de CombatService */
            builder.Services.AddScoped<ICombatService, CombatService>();
            builder.Services.AddScoped<IInstanceMonstreService, InstanceMonsterService>();
            builder.Services.AddScoped<IProgressionService, ProgressionService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Initialisation : Générer 300 instances de monstres au démarrage
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var monstreService = services.GetRequiredService<IInstanceMonstreService>();
                    var context = services.GetRequiredService<MyLittleRPGContext>();
                    
                    // RÉINITIALISATION : Suppression de tous les anciens monstres
                    int supprimes = monstreService.SupprimerTousLesMonstresAsync().GetAwaiter().GetResult();
                    Console.WriteLine($"Monstres supprimés : {supprimes}");
                    
                    // RÉINITIALISATION : Remettre tous les personnages à leur état initial
                    var personnages = context.Personnages.ToList();
                    foreach (var perso in personnages)
                    {
                        perso.PointsVie = 100;
                        perso.PointsVieMax = 100;
                        perso.PositionX = 10;
                        perso.PositionY = 10;
                        perso.Experience = 0;
                        perso.Niveau = 1;
                        perso.Force = 20;
                        perso.Defense = 12;
                    }
                    context.SaveChanges();
                    Console.WriteLine($"Personnages réinitialisés : {personnages.Count}");
                    
                    int nombreMonstres = context.InstancesMonstres.Count();
                    Console.WriteLine($"Nombre de monstres actuels : {nombreMonstres}");
                    
                    int crees = monstreService.AssurerNombreAsync(300).GetAwaiter().GetResult();
                    Console.WriteLine($"{crees} nouveaux monstres générés. Total : {context.InstancesMonstres.Count()}");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "❌ Erreur lors de l'initialisation des monstres");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();



            app.Run();
        }
    }
}
