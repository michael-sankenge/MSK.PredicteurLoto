using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MSK.PredicteurLoto
{
    public static class FacebookParser
    {
        public static List<Tirage> AnalyserTexte(string texteBrut)
        {
            var resultats = new List<Tirage>();

            if (string.IsNullOrWhiteSpace(texteBrut))
                return resultats;

            // Pattern Regex pour intercepter le format : "06H30 : 48-55-04-90-72   40-56"
            var pattern = @"(?<heure>\d{2}H\d{2})\s*:\s*(?<b1>\d+)-(?<b2>\d+)-(?<b3>\d+)-(?<b4>\d+)-(?<b5>\d+)(?:\s+(?<e1>\d+)-(?<e2>\d+))?";
            var matches = Regex.Matches(texteBrut, pattern);

            foreach (Match match in matches)
            {
                resultats.Add(new Tirage
                {
                    Heure = match.Groups["heure"].Value,
                    Boule1 = int.Parse(match.Groups["b1"].Value),
                    Boule2 = int.Parse(match.Groups["b2"].Value),
                    Boule3 = int.Parse(match.Groups["b3"].Value),
                    Boule4 = int.Parse(match.Groups["b4"].Value),
                    Boule5 = int.Parse(match.Groups["b5"].Value),
                    Extra1 = match.Groups["e1"].Success ? int.Parse(match.Groups["e1"].Value) : 0,
                    Extra2 = match.Groups["e2"].Success ? int.Parse(match.Groups["e2"].Value) : 0
                });
            }

            return resultats;
        }
    }
}