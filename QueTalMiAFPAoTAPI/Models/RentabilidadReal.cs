namespace QueTalMiAFPAoTAPI.Models {
    public record RentabilidadReal(
        string Afp,
        string Fondo,
        decimal ValorCuotaInicial,
        decimal ValorUfInicial,
        decimal ValorCuotaFinal,
        decimal ValorUfFinal,
        decimal Rentabilidad
    );
}
