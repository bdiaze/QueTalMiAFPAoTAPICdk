namespace QueTalMiAFPAoTAPI.Entities {
    public class ApiKey {
        public long? Id { get; set; }

        public required string RequestId { get; set; }

        public required string Sub { get; set; }

        public required string ApiKeyPublicId { get; set; }

        public required string ApiKeyHash { get; set; }

        public required DateTimeOffset FechaCreacion { get; set; }

        public DateTimeOffset? FechaEliminacion { get; set; }

        public required short Vigente { get; set; }

        // public required int QuoteLimite { get; set; }

        // public required int QuoteCompensacion { get; set; }

        // public required short QuotePeriodo { get; set; } /* 1: DIARIO - 2: SEMANAL - 3: MENSUAL */

        // public required int ThrottleTasa { get; set; }

        // public required int  ThrottleBurst { get; set; }

        public DateTimeOffset? FechaUltimoUso { get; set; }

        // public required int UsoPeriodo { get; set; }
    }
}
