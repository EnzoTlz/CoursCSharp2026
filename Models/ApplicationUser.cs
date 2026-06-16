using Microsoft.AspNetCore.Identity;

namespace Eval.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }

        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}