using Amazon.S3.Model;

namespace QueTalMiAFPAoTAPI.Models {
    public class SalCrearApiKey {
        public required long Id { get; set; }
        public string? ApiKey { get; set; }
    }
}
