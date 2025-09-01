namespace QueTalMiAFPAoTAPI.Models {
    public class RentabilidadReal { 
        public required string Afp { get; set; }
        public required string Fondo { get; set; }
        public required decimal ValorCuotaInicial { get; set; }
        public required decimal ValorUfInicial { get; set; }
        public required decimal ValorCuotaFinal { get; set; }
        public required decimal ValorUfFinal { get; set; }
        public required decimal Rentabilidad { get; set; }
    }
}
