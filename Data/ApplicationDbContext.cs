using Eval.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eval.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Match> Matches { get; set; }
        public DbSet<Prediction> Predictions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Un seul prono par utilisateur et par match
            builder.Entity<Prediction>().HasIndex(p => new { p.UserId, p.MatchId }).IsUnique();
        }
    }
}