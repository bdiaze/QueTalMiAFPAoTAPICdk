namespace QueTalMiAFPAoTAPI.Entities {
    public class Notificacion {
        public long? Id { get; set; }

        public required string Sub { get; set; }

        public required string CorreoNotificacion { get; set; }

        public required short IdTipoNotificacion { get; set; }

        public required DateTime FechaCreacion { get; set; }

        public DateTime? FechaEliminacion { get; set; }

        public required short Vigente { get; set; }

        public required DateTime FechaHabilitacion { get; set; }

        public DateTime? FechaDeshabilitacion { get; set; }

        public required short Habilitado { get; set; }
    }
}
