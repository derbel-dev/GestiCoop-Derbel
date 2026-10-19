using System;

namespace GestiCoop.Models
{
    public class OffreStage
    {
        public string Entreprise { get; set; } = "";
        public string Poste { get; set; } = "";
        public string Programme { get; set; } = "";
        public string TypeStage { get; set; } = "";
        public string Lieu { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int NbPostes { get; set; } = 1;
        public string Description { get; set; } = "";
        public string Statut { get; set; } = "Ouvert";
    }
}