namespace QueTalMiAFPAoTAPI.Models {
    public record SalObtenerUltimaCuota(
        string Afp,
        DateTime Fecha,
        string Fondo,
        decimal Valor,
        decimal? Comision
    );
}
