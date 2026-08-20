using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MSK.PredicteurLoto
{
    public static class StorageService
    {
        private static readonly string FichierSauvegarde = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "historique_tirages.json"
        );

        private static readonly JsonSerializerOptions OptionsJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static void Sauvegarder(List<Tirage> tirages)
        {
            try
            {
                string json = JsonSerializer.Serialize(tirages, OptionsJson);
                File.WriteAllText(FichierSauvegarde, json);
            }
            catch
            {
                // Ignorer ou loguer les erreurs d'écriture
            }
        }

        public static List<Tirage> Charger()
        {
            try
            {
                if (File.Exists(FichierSauvegarde))
                {
                    string json = File.ReadAllText(FichierSauvegarde);
                    return JsonSerializer.Deserialize<List<Tirage>>(json, OptionsJson) ?? new List<Tirage>();
                }
            }
            catch
            {
                // En cas de fichier corrompu ou illisible, réinitialiser sans planter
            }

            return new List<Tirage>();
        }
    }
}