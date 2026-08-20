using System;
using System.Collections.Generic;
using System.Linq;

namespace MSK.PredicteurLoto
{
    public static class PronosticService
    {
        public static List<Tirage> GenererPronostics(List<Tirage> historique, int mode)
        {
            var resultats = new List<Tirage>();
            if (historique == null || !historique.Any()) return resultats;

            // Calcul des fréquences de chaque numéro (1 à 90)
            var frequences = Enumerable.Range(1, 90)
                .Select(n => new
                {
                    Numero = n,
                    Count = historique.Count(t => t.Boule1 == n || t.Boule2 == n || t.Boule3 == n || t.Boule4 == n || t.Boule5 == n)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var rand = new Random();

            for (int i = 1; i <= 5; i++)
            {
                List<int> selection;

                if (mode == 0) // Mode Turbo (Numéros Chauds)
                {
                    // Prendre parmi le TOP 20 des plus fréquents + un peu de hasard
                    selection = frequences.Take(20).Select(x => x.Numero).OrderBy(_ => rand.Next()).Take(7).OrderBy(n => n).ToList();
                }
                else if (mode == 1) // Mode Équilibré (Chauds + Froids)
                {
                    var chauds = frequences.Take(15).Select(x => x.Numero).OrderBy(_ => rand.Next()).Take(4);
                    var froids = frequences.TakeLast(20).Select(x => x.Numero).OrderBy(_ => rand.Next()).Take(3);
                    selection = chauds.Concat(froids).Distinct().OrderBy(n => n).ToList();

                    // Sécurité si moins de 7 numéros
                    while (selection.Count < 7)
                    {
                        int next = rand.Next(1, 91);
                        if (!selection.Contains(next)) selection.Add(next);
                    }
                    selection.Sort();
                }
                else // Système Réducteur
                {
                    selection = frequences.Select(x => x.Numero).OrderBy(_ => rand.Next()).Take(7).OrderBy(n => n).ToList();
                }

                // 5 boules principales + 2 numéros Extra
                resultats.Add(new Tirage
                {
                    Heure = $"PRONO #{i}",
                    Boule1 = selection[0],
                    Boule2 = selection[1],
                    Boule3 = selection[2],
                    Boule4 = selection[3],
                    Boule5 = selection[4],
                    Extra1 = selection[5], // 6e numéro généré
                    Extra2 = selection[6]  // 7e numéro généré
                });
            }

            return resultats;
        }
    }
}