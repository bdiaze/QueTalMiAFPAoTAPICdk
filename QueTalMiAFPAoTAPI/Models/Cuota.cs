namespace QueTalMiAFPAoTAPI.Models {
    public record Cuota(
    long Id,
    string Afp,
    DateTime Fecha,
    string Fondo,
    decimal Valor
    );
}
