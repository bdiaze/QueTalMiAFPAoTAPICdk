namespace QueTalMiAFPAoTAPI.Entities {
    public class HistorialUsoApiKey {
        public long? Id { get; set; }
        public required long IdApiKey { get; set; }
        public required DateTimeOffset FechaUso { get; set; }
        public required string Ruta { get; set; }
        public required string ParametrosEntrada { get; set; }
        public required short CodigoRetorno { get; set; }
        public required int CantRegistrosRetorno { get; set; }
    }
}
