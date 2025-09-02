namespace QueTalMiAFPAoTAPI.Entities {
    public class HistorialNotificaciones {
        public long? Id { get; set; }

        public required long IdNotificacion { get; set; }

        public required DateTime FechaNotificacion { get; set; }

        public required short Estado { get; set; }
    }
}
