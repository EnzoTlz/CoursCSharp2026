using System.Security.Claims;
using Eval.Data;
using Eval.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eval.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Liste des matchs avec saisie des scores
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches
                .OrderBy(m => m.KickoffUtc)
                .ToListAsync();

            return View(matches);
        }

        // Enregistre le score réel d'un match et recalcule les points
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetScore(int matchId, int scoreA, int scoreB)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
            {
                return NotFound();
            }

            match.ScoreA = scoreA;
            match.ScoreB = scoreB;
            await _context.SaveChangesAsync();

            // Recalcule les points de tous les pronos de ce match
            await RecomputePointsForMatchAsync(matchId);

            return RedirectToAction(nameof(Index));
        }

        private async Task RecomputePointsForMatchAsync(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null || !match.IsPlayed)
            {
                return;
            }

            var predictions = await _context.Predictions
                .Where(p => p.MatchId == matchId)
                .ToListAsync();

            foreach (var p in predictions)
            {
                p.PointsAwarded = ScoringService.ComputePoints(
                    p.PredictedScoreA, p.PredictedScoreB,
                    match.ScoreA!.Value, match.ScoreB!.Value);
            }

            await _context.SaveChangesAsync();
        }
    }
}