namespace QueTalMiAFPAoTAPI.Models {
    public class SalObtenerUltimaCuota { 
        public required string Afp { get; set; }
        public required DateOnly Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
        public DateOnly? FechaUf { get; set; }
        public decimal? ValorUf { get; set; }
        public decimal? Comision { get; set; }
    }
}
