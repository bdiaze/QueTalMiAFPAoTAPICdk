namespace QueTalMiAFPAoTAPI.Models {
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

    public record Comision(
        long Id,
        string Afp,
        DateTime Fecha,
        decimal Valor,
        byte TipoComision,
        byte TipoValor
    );
}
