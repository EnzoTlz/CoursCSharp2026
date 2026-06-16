namespace Eval.Services
{
    public static class ScoringService
    {
        // Calcule les points d'un prono face au score réel
        // 3 = score exact, 1 = bon résultat (vainqueur ou nul), 0 = raté
        public static int ComputePoints(int predictedA, int predictedB, int realA, int realB)
        {
            // Score exact
            if (predictedA == realA && predictedB == realB)
            {
                return 3;
            }

            // Bon résultat : on compare les "issues" (signe de la différence)
            var predictedOutcome = Math.Sign(predictedA - predictedB);
            var realOutcome = Math.Sign(realA - realB);

            if (predictedOutcome == realOutcome)
            {
                return 1;
            }

            return 0;
        }
    }
}