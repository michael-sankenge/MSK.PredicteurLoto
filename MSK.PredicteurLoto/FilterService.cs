using System.Collections.Generic;
using System.Linq;

namespace MSK.PredicteurLoto
{
    public static class FilterService
    {
        public static bool EstCombinaisonValide(List<int> grille)
        {
            if (grille == null || grille.Count != 5)
                return false;

            // 1. Filtre Pairs/Impairs (Rejeter 5-0 et 0-5)
            int nbPairs = grille.Count(n => n % 2 == 0);
            if (nbPairs == 0 || nbPairs == 5)
                return false;

            // 2. Filtre de la Somme Totale (Plage 120 - 330)
            int somme = grille.Sum();
            if (somme < 120 || somme > 330)
                return false;

            // 3. Filtre des Suites Consécutives (Max 2 numéros qui se suivent)
            var triee = grille.OrderBy(n => n).ToList();
            int maxConsecutifs = 1;
            int courantConsecutif = 1;

            for (int i = 0; i < triee.Count - 1; i++)
            {
                if (triee[i + 1] == triee[i] + 1)
                {
                    courantConsecutif++;
                    if (courantConsecutif > maxConsecutifs)
                        maxConsecutifs = courantConsecutif;
                }
                else
                {
                    courantConsecutif = 1;
                }
            }

            if (maxConsecutifs > 2)
                return false;

            return true;
        }
    }
}