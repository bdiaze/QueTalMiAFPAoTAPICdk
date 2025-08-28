namespace QueTalMiAFPAoTAPI.Models {
    public record CuotaUf(
        string Afp,
        string Fecha,
        string Fondo,
        decimal Valor,
        decimal? ValorUf
    );
}
