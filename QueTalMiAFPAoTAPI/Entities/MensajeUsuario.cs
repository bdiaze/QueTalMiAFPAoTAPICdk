namespace QueTalMiAFPAoTAPI.Entities {
    public class MensajeUsuario { 
        public long? IdMensaje { get; set; }
        public required short IdTipoMensaje { get; set; }
        public required DateTime FechaIngreso { get; set; }
        public required string Nombre { get; set; }
        public required string Correo { get; set; }
        public required string Mensaje { get; set; }
        public TipoMensaje? TipoMensaje { get; set; }
    }
}
