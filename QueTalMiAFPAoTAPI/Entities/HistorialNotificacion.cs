namespace QueTalMiAFPAoTAPI.Entities {
    public class HistorialNotificacion {
        public long? Id { get; set; }

        public required long IdNotificacion { get; set; }

        public required DateTimeOffset FechaNotificacion { get; set; }

        public required short Estado { get; set; }
    }
}
