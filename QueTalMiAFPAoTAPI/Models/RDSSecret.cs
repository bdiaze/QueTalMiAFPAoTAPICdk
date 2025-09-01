namespace QueTalMiAFPAoTAPI.Models {
    public class RDSSecret { 
        public required string Host { get; set; }
        public required string Port { get; set; }
        public required string QueTalMiAFPDatabase { get; set; }
        public required string QueTalMiAFPAppUsername { get; set; }
        public required string QueTalMiAFPAppPassword { get; set; }
    }
}
