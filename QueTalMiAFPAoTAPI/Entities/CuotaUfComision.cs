namespace QueTalMiAFPAoTAPI.Entities {
    public class CuotaUfComision { 
        public required string Afp { get; set; }
        public required DateOnly Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
        public decimal? ValorUf { get; set; }
        public decimal? ComisDeposCotizOblig { get; set; }
        public decimal? ComisAdminCtaAhoVol { get; set; }
    }
}
