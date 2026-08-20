using System.Collections.Generic;
using System.Linq;

namespace MSK.PredicteurLoto
{
    public static class StatistiquesService
    {
        public static (string chauds, string froids) ObtenirStats(List<Tirage> tirages)
        {
            if (tirages == null || !tirages.Any())
                return ("🔥 Top Chauds : Aucun", "🧊 Top Froids : Aucun");

            var frequences = Enumerable.Range(1, 90).ToDictionary(n => n, n => 0);

            foreach (var t in tirages)
            {
                var boules = new[] { t.Boule1, t.Boule2, t.Boule3, t.Boule4, t.Boule5 };
                foreach (var b in boules)
                {
                    if (frequences.ContainsKey(b))
                        frequences[b]++;
                }
            }

            var topChauds = frequences.OrderByDescending(f => f.Value).Take(5).Select(f => f.Key);
            var topFroids = frequences.OrderBy(f => f.Value).Take(5).Select(f => f.Key);

            return (
                $"🔥 Top Chauds : {string.Join(" - ", topChauds)}",
                $"🧊 Top Froids : {string.Join(" - ", topFroids)}"
            );
        }

        public static List<StatNumero> ObtenirTableauAnalyse(List<Tirage> tirages)
        {
            var resultat = new List<StatNumero>();
            if (tirages == null || !tirages.Any())
                return resultat;

            for (int num = 1; num <= 90; num++)
            {
                int freq = 0;
                int ecart = 0;
                bool trouveDernier = false;

                for (int i = tirages.Count - 1; i >= 0; i--)
                {
                    var t = tirages[i];
                    var boules = new[] { t.Boule1, t.Boule2, t.Boule3, t.Boule4, t.Boule5 };

                    if (boules.Contains(num))
                    {
                        freq++;
                        if (!trouveDernier)
                            trouveDernier = true;
                    }
                    else if (!trouveDernier)
                    {
                        ecart++;
                    }
                }

                string statut = "⚖️ Neutre";
                if (freq >= 3) statut = "🔥 Chaud";
                else if (ecart >= 5) statut = "🧊 En Retard";

                resultat.Add(new StatNumero
                {
                    Numero = num,
                    Frequence = freq,
                    EcartActuel = ecart,
                    Statut = statut
                });
            }

            return resultat.OrderByDescending(s => s.Frequence).ThenBy(s => s.EcartActuel).ToList();
        }
    }
}