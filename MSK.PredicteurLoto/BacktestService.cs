using System;
using System.Collections.Generic;
using System.Linq;

namespace MSK.PredicteurLoto
{
    public class BacktestResultat
    {
        public int TotalTiragesTestes { get; set; }
        public int Gain2Boules { get; set; }
        public int Gain3Boules { get; set; }
        public int Gain4Boules { get; set; }
        public int Gain5Boules { get; set; }
        public double TauxReussiteGlobal { get; set; }

        public string ObtenirMessageFormatte()
        {
            if (TotalTiragesTestes == 0)
                return "⚠️ Pas assez de tirages pour effectuer le test (minimum 6 tirages requis).";

            return $"📊 Backtest sur {TotalTiragesTestes} tirages : " +
                   $"2 bons = {Gain2Boules} | 3 bons = {Gain3Boules} | 4 bons = {Gain4Boules} | 5 bons = {Gain5Boules} " +
                   $"({TauxReussiteGlobal:F1}% de grilles gagnantes)";
        }
    }

    public static class BacktestService
    {
        public static BacktestResultat ExecuterTest(List<Tirage> historique)
        {
            var res = new BacktestResultat();
            if (historique == null || historique.Count < 6)
                return res;

            int totalTests = 0;

            // Parcours glissant : on prédit le tirage [i] à partir des tirages antérieurs [0..i-1]
            for (int i = 5; i < historique.Count; i++)
            {
                var passe = historique.Take(i).ToList();
                var tirageReel = historique[i];

                var boulesReelles = new HashSet<int>
                {
                    tirageReel.Boule1, tirageReel.Boule2, tirageReel.Boule3, tirageReel.Boule4, tirageReel.Boule5
                };

                // Génération des pronostics basés sur le passé
                var pronos = PronosticService.GenererPronostics(passe, 0);

                foreach (var p in pronos)
                {
                    // Évaluation des 7 numéros du pronostic (5 principales + 2 extras si présents)
                    var pronoBoules = new List<int> { p.Boule1, p.Boule2, p.Boule3, p.Boule4, p.Boule5 };
                    if (p.Extra1 > 0) pronoBoules.Add(p.Extra1);
                    if (p.Extra2 > 0) pronoBoules.Add(p.Extra2);

                    int correspondances = pronoBoules.Count(b => boulesReelles.Contains(b));

                    if (correspondances == 2) res.Gain2Boules++;
                    else if (correspondances == 3) res.Gain3Boules++;
                    else if (correspondances == 4) res.Gain4Boules++;
                    else if (correspondances >= 5) res.Gain5Boules++;
                }

                totalTests++;
            }

            res.TotalTiragesTestes = totalTests;
            int totalGrillesGagnantes = res.Gain2Boules + res.Gain3Boules + res.Gain4Boules + res.Gain5Boules;
            int totalGrillesJouees = totalTests * 5; // 5 grilles PRONO générées par tirage

            res.TauxReussiteGlobal = totalGrillesJouees > 0 ? (double)totalGrillesGagnantes / totalGrillesJouees * 100 : 0;

            return res;
        }
    }
}