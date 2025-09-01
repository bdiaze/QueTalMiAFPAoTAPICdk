namespace QueTalMiAFPAoTAPI.Models {
    public class EntObtenerUltimaCuota { 
        public required string ListaAFPs { get; set; }
        public required string ListaFondos { get; set; }
        public required string ListaFechas { get; set; }
        public required int TipoComision { get; set; }
    }
}
