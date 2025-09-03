namespace QueTalMiAFPAoTAPI.Models {
    public class EntIngresarHistorialNotificacion {
        public required long IdNotificacion { get; set; }
        public required DateTimeOffset FechaNotificacion { get; set; }
        public required short Estado { get; set; }
    }
}
