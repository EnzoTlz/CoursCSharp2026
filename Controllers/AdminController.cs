using System.Security.Claims;
using Eval.Data;
using Eval.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        // Créer un nouveau match
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatch(string teamA, string teamB, DateTime kickoff)
        {
            if (string.IsNullOrWhiteSpace(teamA) || string.IsNullOrWhiteSpace(teamB))
            {
                return RedirectToAction(nameof(Index));
            }

            var match = new Eval.Models.Match
            {
                TeamA = teamA.Trim(),
                TeamB = teamB.Trim(),
                KickoffUtc = kickoff
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Supprimer un match (et ses pronostics liés, via la cascade)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMatch(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match != null)
            {
                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromApi()
        {
            var url = "https://raw.githubusercontent.com/openfootball/worldcup.json/master/2026/worldcup.json";

            using var httpClient = new HttpClient();
            string json;
            try
            {
                json = await httpClient.GetStringAsync(url);
            }
            catch
            {
                TempData["ImportError"] = "Impossible de contacter l'API. Réessaie plus tard.";
                return RedirectToAction(nameof(Index));
            }

            var data = JsonSerializer.Deserialize<Eval.Models.WorldCupData>(json);
            if (data == null || data.Matches.Count == 0)
            {
                TempData["ImportError"] = "Aucune donnée reçue de l'API.";
                return RedirectToAction(nameof(Index));
            }

            // On charge les matchs déjà en base une seule fois
            var existingMatches = await _context.Matches.ToListAsync();

            int added = 0;
            int updated = 0;
            var matchesToRecompute = new List<int>();

            foreach (var m in data.Matches)
            {
                // --- Parsing de l'heure (comme avant) ---
                DateTime kickoff;
                var rawTime = (m.Time ?? "").Trim();
                var parts = rawTime.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var timeOnly = parts.Length > 0 ? parts[0] : "00:00";

                int offsetHours = 0;
                if (parts.Length > 1 && parts[1].StartsWith("UTC"))
                {
                    int.TryParse(parts[1].Substring(3), out offsetHours);
                }

                if (DateTime.TryParse($"{m.Date} {timeOnly}", out var localStadium))
                {
                    kickoff = localStadium.AddHours(-offsetHours + 2);
                }
                else
                {
                    kickoff = DateTime.Now;
                }

                // --- Score de l'API (peut être null) ---
                int? scoreA = m.Score?.Ft != null && m.Score.Ft.Count == 2 ? m.Score.Ft[0] : null;
                int? scoreB = m.Score?.Ft != null && m.Score.Ft.Count == 2 ? m.Score.Ft[1] : null;

                // --- On cherche le match correspondant en base (mêmes équipes) ---
                var existing = existingMatches.FirstOrDefault(e =>
                    e.TeamA == m.Team1 && e.TeamB == m.Team2);

                if (existing == null)
                {
                    // Match inconnu -> on l'ajoute
                    _context.Matches.Add(new Eval.Models.Match
                    {
                        TeamA = m.Team1,
                        TeamB = m.Team2,
                        KickoffUtc = kickoff,
                        ScoreA = scoreA,
                        ScoreB = scoreB
                    });
                    added++;
                }
                else
                {
                    // Match connu -> on met à jour la date, et le score SEULEMENT si l'API en a un
                    existing.KickoffUtc = kickoff;

                    if (scoreA.HasValue && scoreB.HasValue)
                    {
                        // On ne réécrit que si le score a changé (évite des recalculs inutiles)
                        if (existing.ScoreA != scoreA || existing.ScoreB != scoreB)
                        {
                            existing.ScoreA = scoreA;
                            existing.ScoreB = scoreB;
                            matchesToRecompute.Add(existing.Id);
                            updated++;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Recalcul des points pour les matchs dont le score vient de changer
            foreach (var id in matchesToRecompute)
            {
                await RecomputePointsForMatchAsync(id);
            }

            TempData["ImportOk"] = $"Import terminé : {added} match(s) ajouté(s), {updated} score(s) mis à jour. Pronostics conservés.";
            return RedirectToAction(nameof(Index));
        }
    }
}