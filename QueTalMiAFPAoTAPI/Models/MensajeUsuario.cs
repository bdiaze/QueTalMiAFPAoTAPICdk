namespace QueTalMiAFPAoTAPI.Models {
    public record MensajeUsuario(
        long IdMensaje,
        short IdTipoMensaje,
        DateTime FechaIngreso,
        string Nombre,
        string Correo,
        string Mensaje,
        TipoMensaje? TipoMensaje
    );
}
