namespace QueTalMiAFPAoTAPI.Models {
    public class SalObtenerUltimaCuota { 
        public required string Afp { get; set; }
        public required DateTime Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
        public decimal? ValorUf { get; set; }
        public decimal? Comision { get; set; }
    }
}
