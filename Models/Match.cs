using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eval.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Required]
        public string TeamA { get; set; } = string.Empty;

        [Required]
        public string TeamB { get; set; } = string.Empty;

        public DateTime KickoffUtc { get; set; }

        // Score réel — null tant que le match n'est pas joué
        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }

        // Calcul pratique, PAS une colonne en base (d'où le [NotMapped])
        [NotMapped]
        public bool IsPlayed => ScoreA.HasValue && ScoreB.HasValue;

        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}