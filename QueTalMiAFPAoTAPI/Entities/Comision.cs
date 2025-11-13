namespace QueTalMiAFPAoTAPI.Entities {
    public enum TipoValorComision: byte {
        Porcentaje = 1,
        PesoChileno = 2
    }

    public enum TipoComision : byte {
        DeposCotizOblig = 1,
        AdminCtaAhoVol = 2,
        AdminAhoPreVol = 3,
        TransAhoPreVol = 4
    }

    public class Comision {
        public long? Id { get; set; }
        public required string Afp { get; set; }
        public required DateOnly Fecha { get; set; }
        public required decimal Valor { get; set; }
        public required byte TipoComision { get; set; }
        public required byte TipoValor { get; set; }
    }
    
}
