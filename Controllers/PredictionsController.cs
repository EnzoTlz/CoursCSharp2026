using System.Security.Claims;
using Eval.Data;
using Eval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eval.Controllers
{
    [Authorize]
    public class PredictionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredictionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // La page qui hébergera l'îlot React
        public IActionResult Index()
        {
            return View();
        }

        // GET : renvoie les matchs à venir + le prono existant de l'utilisateur
        [HttpGet]
        public async Task<IActionResult> Upcoming()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;

            var matches = await _context.Matches
                .Where(m => m.KickoffUtc > now)
                .OrderBy(m => m.KickoffUtc)
                .Select(m => new
                {
                    m.Id,
                    m.TeamA,
                    m.TeamB,
                    Kickoff = m.KickoffUtc,
                    // Le prono de CET utilisateur pour CE match, s'il existe
                    Prediction = m.Predictions
                        .Where(p => p.UserId == userId)
                        .Select(p => new { p.PredictedScoreA, p.PredictedScoreB })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Json(matches);
        }

        // POST : enregistre ou met à jour un prono
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] PredictionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Vérifier que le match existe et n'a pas démarré
            var match = await _context.Matches.FindAsync(dto.MatchId);
            if (match == null)
            {
                return NotFound();
            }
            if (match.KickoffUtc <= DateTime.Now)
            {
                return BadRequest("Le match a déjà commencé.");
            }

            // Chercher un prono existant de cet utilisateur pour ce match
            var prediction = await _context.Predictions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == dto.MatchId);

            if (prediction == null)
            {
                // Créer
                prediction = new Prediction
                {
                    UserId = userId!,
                    MatchId = dto.MatchId,
                    PredictedScoreA = dto.ScoreA,
                    PredictedScoreB = dto.ScoreB
                };
                _context.Predictions.Add(prediction);
            }
            else
            {
                // Mettre à jour
                prediction.PredictedScoreA = dto.ScoreA;
                prediction.PredictedScoreB = dto.ScoreB;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Page de consultation : mes pronos sur les matchs déjà commencés
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;

            var predictions = await _context.Predictions
                .Where(p => p.UserId == userId && p.Match.KickoffUtc <= now)
                .Include(p => p.Match)
                .OrderByDescending(p => p.Match.KickoffUtc)
                .ToListAsync();

            return View(predictions);
        }

        // Recalcule et enregistre les points de tous les pronos d'un match donné
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
                p.PointsAwarded = Eval.Services.ScoringService.ComputePoints(
                    p.PredictedScoreA, p.PredictedScoreB,
                    match.ScoreA!.Value, match.ScoreB!.Value);
            }

            await _context.SaveChangesAsync();
        }

        // TEMPORAIRE — recalcule les points de tous les matchs joués (pour tester)
        public async Task<IActionResult> RecomputeAll()
        {
            var playedMatchIds = await _context.Matches
                .Where(m => m.ScoreA != null && m.ScoreB != null)
                .Select(m => m.Id)
                .ToListAsync();

            foreach (var id in playedMatchIds)
            {
                await RecomputePointsForMatchAsync(id);
            }

            return Content("Points recalculés pour tous les matchs joués.");
        }

        // Petit objet pour recevoir les données du POST
        public class PredictionDto
        {
            public int MatchId { get; set; }
            public int ScoreA { get; set; }
            public int ScoreB { get; set; }
        }
    }
}