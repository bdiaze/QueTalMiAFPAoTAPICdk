namespace QueTalMiAFPAoTAPI.Models {
    public class EntIngresarTipoNotificacion {
        public required short Id { get; set; }
        public required string Nombre { get; set; } 
        public required string Descripcion { get; set; }
        public required short IdTipoPeriodicidad { get; set; }
        public required short Habilitado { get; set; }
    }
}
