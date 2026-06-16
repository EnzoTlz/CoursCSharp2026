using Eval.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eval.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ranking = await _context.Predictions
                .Include(p => p.User)
                .GroupBy(p => new { p.UserId, p.User.DisplayName, p.User.Email })
                .Select(g => new LeaderboardRow
                {
                    DisplayName = g.Key.DisplayName ?? g.Key.Email,
                    TotalPoints = g.Sum(p => p.PointsAwarded),
                    PredictionsCount = g.Count()
                })
                .OrderByDescending(r => r.TotalPoints)
                .ToListAsync();

            return View(ranking);
        }

        public class LeaderboardRow
        {
            public string DisplayName { get; set; } = string.Empty;
            public int TotalPoints { get; set; }
            public int PredictionsCount { get; set; }
        }
    }
}