namespace QueTalMiAFPAoTAPI.Models {
    public record EntObtenerUltimaCuota(
        string ListaAFPs,
        string ListaFondos,
        string ListaFechas,
        int TipoComision
    );
}
