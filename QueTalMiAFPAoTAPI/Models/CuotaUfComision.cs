namespace QueTalMiAFPAoTAPI.Models {
    public record CuotaUfComision(
        string Afp,
        DateTime Fecha,
        string Fondo,
        decimal Valor,
        decimal? ValorUf,
        decimal? ComisDeposCotizOblig,
        decimal? ComisAdminCtaAhoVol
    );
}
