namespace QueTalMiAFPAoTAPI.Models {
    public record EntIngresarMensaje(
        short IdTipoMensaje,
        string Nombre,
        string Correo,
        string Mensaje
    );
}
