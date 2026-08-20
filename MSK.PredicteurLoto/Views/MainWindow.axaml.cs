using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MSK.PredicteurLoto.Views
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Tirage> ListeTirages { get; set; } = new ObservableCollection<Tirage>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ChargerDonneesEnregistrees();
        }

        private void ChargerDonneesEnregistrees()
        {
            var enregistrements = StorageService.Charger();
            RechargerGrille(enregistrements);
        }

        private void RechargerGrille(List<Tirage> tirages)
        {
            ListeTirages.Clear();
            foreach (var t in tirages)
            {
                ListeTirages.Add(t);
            }

            RafraichirDataGrid();
            ActualiserStatistiques();
        }

        private void RafraichirDataGrid()
        {
            if (GridTirages != null)
            {
                GridTirages.Items = null;
                GridTirages.Items = ListeTirages;
            }
        }

        // 1. Importation Facebook
        public void OnImportClick(object? sender, RoutedEventArgs e)
        {
            string texte = TxtFacebookInput?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texte))
            {
                if (TxtStats != null)
                    TxtStats.Text = "⚠️ Collez d'abord le texte Facebook dans la zone ci-dessus.";
                return;
            }

            var nouveauxTirages = FacebookParser.AnalyserTexte(texte);

            if (nouveauxTirages != null && nouveauxTirages.Any())
            {
                var historique = StorageService.Charger();
                historique.AddRange(nouveauxTirages);
                StorageService.Sauvegarder(historique);

                RechargerGrille(historique);

                if (TxtFacebookInput != null)
                    TxtFacebookInput.Clear();

                if (TxtStats != null)
                    TxtStats.Text = $"🚀 {nouveauxTirages.Count} tirages importés avec succès ! Total : {historique.Count} tirages en mémoire.";
            }
            else
            {
                if (TxtStats != null)
                    TxtStats.Text = "⚠️ Aucun tirage valide n'a pu être détecté dans le texte copié.";
            }
        }

        // 2. Saisie Manuelle Directe (Prend en charge 5, 6 ou 7 numéros)
        public void OnAjouterSaisieManuelleClick(object? sender, RoutedEventArgs e)
        {
            string saisie = TxtSaisieManuelle?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(saisie))
            {
                if (TxtStats != null)
                    TxtStats.Text = "⚠️ Entrez 5 à 7 numéros séparés par des espaces (ex: 12 45 67 89 2 14 88).";
                return;
            }

            var parties = saisie.Split(new[] { ' ', ',', ';', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parties.Length >= 5)
            {
                int maxNums = Math.Min(parties.Length, 7);
                int[] nums = new int[maxNums];
                bool valide = true;

                for (int i = 0; i < maxNums; i++)
                {
                    if (!int.TryParse(parties[i], out nums[i]) || nums[i] < 1 || nums[i] > 90)
                    {
                        valide = false;
                        break;
                    }
                }

                if (valide)
                {
                    var historique = StorageService.Charger();
                    var nouveauTirage = new Tirage
                    {
                        Heure = DateTime.Now.ToString("HH:mm"),
                        Boule1 = nums[0],
                        Boule2 = nums[1],
                        Boule3 = nums[2],
                        Boule4 = nums[3],
                        Boule5 = nums[4],
                        Extra1 = maxNums >= 6 ? nums[5] : 0,
                        Extra2 = maxNums >= 7 ? nums[6] : 0
                    };

                    historique.Add(nouveauTirage);
                    StorageService.Sauvegarder(historique);

                    RechargerGrille(historique);

                    if (TxtSaisieManuelle != null)
                        TxtSaisieManuelle.Clear();

                    if (TxtStats != null)
                        TxtStats.Text = $"✅ Tirage ({nums[0]}-{nums[1]}-{nums[2]}-{nums[3]}-{nums[4]} | Extra: {nouveauTirage.Extra1}-{nouveauTirage.Extra2}) ajouté ! Total : {historique.Count} tirages.";
                }
                else
                {
                    if (TxtStats != null)
                        TxtStats.Text = "⚠️ Les numéros doivent être compris entre 1 et 90.";
                }
            }
            else
            {
                if (TxtStats != null)
                    TxtStats.Text = "⚠️ Veuillez entrer au moins 5 numéros séparés par des espaces.";
            }
        }

        // 3. Génération de Pronostics
        public void OnGenererPronosticClick(object? sender, RoutedEventArgs e)
        {
            var historique = StorageService.Charger();

            if (!historique.Any())
            {
                if (TxtStats != null)
                    TxtStats.Text = "⚠️ Saisissez ou importez d'abord au moins un tirage dans l'historique !";
                return;
            }

            int modeSelectionne = CboModePronostic?.SelectedIndex ?? 0;
            var pronostics = PronosticService.GenererPronostics(historique, modeSelectionne);

            if (pronostics != null && pronostics.Any())
            {
                ListeTirages.Clear();
                foreach (var p in pronostics)
                {
                    ListeTirages.Add(p);
                }

                RafraichirDataGrid();

                if (TxtStats != null)
                    TxtStats.Text = $"🎯 5 Pronostics générés avec succès basés sur {historique.Count} tirages historiques !";
            }
        }

        // 4. Purge des tirages
        public void OnPurgerClick(object? sender, RoutedEventArgs e)
        {
            ListeTirages.Clear();
            StorageService.Sauvegarder(new List<Tirage>());
            RafraichirDataGrid();

            if (TxtStats != null)
            {
                TxtStats.Text = "🧹 Historique vidé avec succès. 🔥 Top Chauds : En attente... | 🧊 Top Froids : En attente...";
            }
        }

        // 5. Exportation en fichier texte (.txt)
        public async void OnExporterClick(object? sender, RoutedEventArgs e)
        {
            if (!ListeTirages.Any())
                return;

            var dialog = new SaveFileDialog
            {
                Title = "Enregistrer les pronostics ou l'historique",
                DefaultExtension = "txt",
                InitialFileName = $"Export_MSK_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            var cheminFichier = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(cheminFichier))
            {
                using var writer = new StreamWriter(cheminFichier);

                await writer.WriteLineAsync("=== MSK PREDICTEUR LOTO 5/90 ===");
                await writer.WriteLineAsync($"Date d'exportation : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                await writer.WriteLineAsync("---------------------------------");

                foreach (var t in ListeTirages)
                {
                    string txtExtra = (t.Extra1 > 0 || t.Extra2 > 0) ? $" | Extra: {t.Extra1} - {t.Extra2}" : "";
                    await writer.WriteLineAsync($"{t.Heure} : {t.Boule1} - {t.Boule2} - {t.Boule3} - {t.Boule4} - {t.Boule5}{txtExtra}");
                }
            }
        }

        // 6. Afficher la table d'analyse des 90 numéros
        public void OnAfficherAnalyseClick(object? sender, RoutedEventArgs e)
        {
            var historique = StorageService.Charger();
            var analyse = StatistiquesService.ObtenirTableauAnalyse(historique);

            ListeTirages.Clear();
            foreach (var stat in analyse.Take(15))
            {
                ListeTirages.Add(new Tirage
                {
                    Heure = $"Num {stat.Numero} ({stat.Statut})",
                    Boule1 = stat.Numero,
                    Boule2 = stat.Frequence,
                    Boule3 = stat.EcartActuel,
                    Boule4 = 0,
                    Boule5 = 0,
                    Extra1 = 0,
                    Extra2 = 0
                });
            }

            RafraichirDataGrid();
        }

        private void ActualiserStatistiques()
        {
            var historique = StorageService.Charger();
            var (chauds, froids) = StatistiquesService.ObtenirStats(historique);
            if (TxtStats != null)
            {
                TxtStats.Text = $"{chauds} | {froids}";
            }
        }
    }
}