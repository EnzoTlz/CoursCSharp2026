using System.ComponentModel.DataAnnotations;

namespace Eval.Models
{
    public class Prediction
    {
        public int Id { get; set; }

        public int PredictedScoreA { get; set; }
        public int PredictedScoreB { get; set; }

        // Points calculés une fois le match joué (0 par défaut)
        public int PointsAwarded { get; set; }

        // Relation vers le match
        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        // Relation vers l'utilisateur (la clé d'Identity est un string)
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}