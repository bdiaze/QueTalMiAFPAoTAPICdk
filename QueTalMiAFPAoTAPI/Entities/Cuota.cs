namespace QueTalMiAFPAoTAPI.Entities {
    public class Cuota { 
        public long? Id { get; set; }
        public required string Afp { get; set; }
        public required DateTime Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
    }
}
