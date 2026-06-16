using Eval.Models;
using Microsoft.AspNetCore.Identity;

namespace Eval.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // 1. Créer le rôle Admin s'il n'existe pas
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Créer le compte admin s'il n'existe pas
            var adminEmail = "admin@admin.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Admin"
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 3. Insérer les matchs s'il n'y en a aucun
            if (!context.Matches.Any())
            {
                context.Matches.AddRange(
                    // Matchs déjà joués (avec score réel)
                    new Match { TeamA = "Mexique", TeamB = "Afrique du Sud", KickoffUtc = new DateTime(2026, 6, 11, 18, 0, 0), ScoreA = 2, ScoreB = 0 },
                    new Match { TeamA = "Brésil", TeamB = "Maroc", KickoffUtc = new DateTime(2026, 6, 12, 21, 0, 0), ScoreA = 1, ScoreB = 1 },
                    new Match { TeamA = "Allemagne", TeamB = "Curaçao", KickoffUtc = new DateTime(2026, 6, 13, 18, 0, 0), ScoreA = 7, ScoreB = 1 },
                    new Match { TeamA = "Pays-Bas", TeamB = "Japon", KickoffUtc = new DateTime(2026, 6, 13, 21, 0, 0), ScoreA = 2, ScoreB = 2 },
                    // Matchs à venir (score réel null = pronostics ouverts)
                    new Match { TeamA = "France", TeamB = "Sénégal", KickoffUtc = new DateTime(2026, 6, 16, 21, 0, 0) },
                    new Match { TeamA = "France", TeamB = "Irak", KickoffUtc = new DateTime(2026, 6, 22, 23, 0, 0) },
                    new Match { TeamA = "Norvège", TeamB = "France", KickoffUtc = new DateTime(2026, 6, 26, 21, 0, 0) }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}