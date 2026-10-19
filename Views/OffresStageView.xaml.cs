using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GestiCoop.Models;

namespace GestiCoop.Views
{
    public partial class OffresStageView : UserControl
    {
        private const string PlaceholderRecherche = "🔍 Rechercher une offre...";

        private readonly ObservableCollection<OffreStage> _offres = new();
        private readonly ICollectionView _vue;
        private OffreStage? _offreEnEdition;

        public OffresStageView()
        {
            InitializeComponent();

            foreach (var s in new[] { "Ubisoft Montréal", "CAE Inc.", "Bell Canada", "Desjardins", "Hydro-Québec" })
                cmbEntreprise.Items.Add(new ComboBoxItem { Content = s });
            foreach (var s in new[] { "Informatique", "Réseautique", "Génie logiciel" })
                cmbProgramme.Items.Add(new ComboBoxItem { Content = s });
            foreach (var s in new[] { "Stage d'été", "Stage de fin d'études" })
                cmbTypeStage.Items.Add(new ComboBoxItem { Content = s });
            cmbStatutInitial.Items.Add(new ComboBoxItem { Content = "En révision" });

            _offres.Add(new OffreStage { Entreprise = "Ubisoft Montréal", Poste = "Développeur junior", Programme = "Informatique", Lieu = "Montréal, QC", DateDebut = new DateTime(2025, 9, 2), DateFin = new DateTime(2026, 4, 24), Statut = "Ouvert" });
            _offres.Add(new OffreStage { Entreprise = "CAE Inc.", Poste = "Analyste de données", Programme = "Informatique", Lieu = "Ottawa, ON", DateDebut = new DateTime(2025, 9, 2), DateFin = new DateTime(2026, 4, 24), Statut = "Ouvert" });
            _offres.Add(new OffreStage { Entreprise = "Bell Canada", Poste = "Intégrateur réseau", Programme = "Informatique", Lieu = "Toronto, ON", DateDebut = new DateTime(2025, 1, 6), DateFin = new DateTime(2025, 4, 18), Statut = "En révision" });
            _offres.Add(new OffreStage { Entreprise = "Desjardins", Poste = "Développeur C#", Programme = "Informatique", Lieu = "Lévis, QC", DateDebut = new DateTime(2024, 9, 3), DateFin = new DateTime(2024, 12, 20), Statut = "Expiré" });
            _offres.Add(new OffreStage { Entreprise = "Hydro-Québec", Poste = "Technicien TI", Programme = "Réseautique", Lieu = "Montréal, QC", DateDebut = new DateTime(2025, 9, 2), DateFin = new DateTime(2026, 4, 24), Statut = "Ouvert" });

            _vue = CollectionViewSource.GetDefaultView(_offres);
            _vue.Filter = FiltrerOffre;
            dgOffres.ItemsSource = _vue;

            cmbEntreprise.SelectionChanged += Champ_Changed;
            cmbProgramme.SelectionChanged += Champ_Changed;
            cmbTypeStage.SelectionChanged += Champ_Changed;
            txtTitrePoste.TextChanged += Champ_Changed;
            txtLieuTravail.TextChanged += Champ_Changed;
            txtDescription.TextChanged += Champ_Changed;
            txtNbPostes.TextChanged += Champ_Changed;
            dpDateDebut.SelectedDateChanged += Champ_Changed;
            dpDateFin.SelectedDateChanged += Champ_Changed;

            btnPublier.Click += Publier_Click;
            btnReinitialiser.Click += Reinitialiser_Click;
            btnPublier.IsEnabled = false;

            txtRecherche.GotFocus += (s, e) => { if (txtRecherche.Text == PlaceholderRecherche) txtRecherche.Clear(); };
            txtRecherche.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtRecherche.Text)) txtRecherche.Text = PlaceholderRecherche; };
            txtRecherche.TextChanged += (s, e) => _vue.Refresh();
            cmbFiltreStatut.SelectionChanged += (s, e) => _vue.Refresh();
        }

        private static string Texte(ComboBox cb) =>
            (cb.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        private static void Selectionner(ComboBox cb, string valeur)
        {
            var item = cb.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Content?.ToString() == valeur);
            cb.SelectedItem = item ?? cb.Items[0];
        }

        private void Champ_Changed(object sender, RoutedEventArgs e)
        {
            if (btnPublier == null) return;

            txtCompteur.Text = $"{txtDescription.Text.Length} / 1000";

            bool datesOk = true;
            if (dpDateDebut.SelectedDate.HasValue && dpDateFin.SelectedDate.HasValue)
                datesOk = dpDateDebut.SelectedDate.Value <= dpDateFin.SelectedDate.Value;
            txtErreurDates.Visibility = datesOk ? Visibility.Collapsed : Visibility.Visible;

            bool postesOk = int.TryParse(txtNbPostes.Text, out int n) && n > 0;

            bool tousRemplis =
                cmbEntreprise.SelectedIndex > 0 &&
                cmbProgramme.SelectedIndex > 0 &&
                !string.IsNullOrWhiteSpace(txtTitrePoste.Text) &&
                !string.IsNullOrWhiteSpace(txtLieuTravail.Text) &&
                !string.IsNullOrWhiteSpace(txtDescription.Text) &&
                dpDateDebut.SelectedDate.HasValue &&
                dpDateFin.SelectedDate.HasValue &&
                postesOk;

            btnPublier.IsEnabled = tousRemplis && datesOk;
        }

        private void Publier_Click(object sender, RoutedEventArgs e)
        {
            var offre = new OffreStage
            {
                Entreprise = Texte(cmbEntreprise),
                Programme = Texte(cmbProgramme),
                TypeStage = Texte(cmbTypeStage),
                Poste = txtTitrePoste.Text.Trim(),
                Lieu = txtLieuTravail.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                DateDebut = dpDateDebut.SelectedDate!.Value,
                DateFin = dpDateFin.SelectedDate!.Value,
                NbPostes = int.Parse(txtNbPostes.Text),
                Statut = Texte(cmbStatutInitial)
            };

            if (_offreEnEdition != null)
            {
                _offres[_offres.IndexOf(_offreEnEdition)] = offre;
                _offreEnEdition = null;
            }
            else
            {
                _offres.Add(offre);
            }

            ViderFormulaire();
            MessageBox.Show("L'offre a été publiée avec succès.", "GestiCoop",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Reinitialiser_Click(object sender, RoutedEventArgs e)
        {
            _offreEnEdition = null;
            ViderFormulaire();
        }

        private void ViderFormulaire()
        {
            cmbEntreprise.SelectedIndex = 0;
            cmbProgramme.SelectedIndex = 0;
            cmbTypeStage.SelectedIndex = 0;
            cmbStatutInitial.SelectedIndex = 0;
            txtTitrePoste.Clear();
            txtLieuTravail.Clear();
            txtDescription.Clear();
            txtNbPostes.Text = "1";
            dpDateDebut.SelectedDate = null;
            dpDateFin.SelectedDate = null;
            Champ_Changed(this, new RoutedEventArgs());
        }

        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OffreStage o) return;

            _offreEnEdition = o;
            Selectionner(cmbEntreprise, o.Entreprise);
            Selectionner(cmbProgramme, o.Programme);
            Selectionner(cmbTypeStage, o.TypeStage);
            Selectionner(cmbStatutInitial, o.Statut);
            txtTitrePoste.Text = o.Poste;
            txtLieuTravail.Text = o.Lieu;
            txtDescription.Text = string.IsNullOrEmpty(o.Description) ? "—" : o.Description;
            txtNbPostes.Text = o.NbPostes.ToString();
            dpDateDebut.SelectedDate = o.DateDebut;
            dpDateFin.SelectedDate = o.DateFin;
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OffreStage o) return;

            var rep = MessageBox.Show($"Supprimer l'offre « {o.Poste} » chez {o.Entreprise} ?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (rep == MessageBoxResult.Yes) _offres.Remove(o);
        }

        private bool FiltrerOffre(object obj)
        {
            if (obj is not OffreStage o) return false;

            string q = txtRecherche.Text == PlaceholderRecherche ? "" : txtRecherche.Text.Trim();
            bool okTexte = q == "" ||
                o.Entreprise.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                o.Poste.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                o.Lieu.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                o.Programme.Contains(q, StringComparison.CurrentCultureIgnoreCase);

            string st = Texte(cmbFiltreStatut);
            bool okStatut = st == "" || st == "Tous les statuts" || o.Statut == st;

            return okTexte && okStatut;
        }
    }
}