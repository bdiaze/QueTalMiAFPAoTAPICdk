namespace QueTalMiAFPAoTAPI.Models {
    public record TipoMensaje(
        short IdTipoMensaje,
        string DescripcionCorta,
        string DescripcionLarga,
        byte Vigencia
    );
}
