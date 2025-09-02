namespace QueTalMiAFPAoTAPI.Entities {
    public class TipoPeriodicidad {
        public required short Id { get; set; }

        public required string Nombre { get; set; }

        public required string Cron { get; set; }
    }
}
