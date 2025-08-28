namespace QueTalMiAFPAoTAPI.Models {
    public record RDSSecret(
        string Host,
        string Port,
        string QueTalMiAFPDatabase,
        string QueTalMiAFPAppUsername,
        string QueTalMiAFPAppPassword
    );
}
