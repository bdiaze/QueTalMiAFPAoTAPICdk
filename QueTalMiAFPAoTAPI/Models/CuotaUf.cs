namespace QueTalMiAFPAoTAPI.Models {
    public class CuotaUf { 
        public required string Afp { get; set; }
        public required string Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
        public decimal? ValorUf { get; set; }
    }
}
