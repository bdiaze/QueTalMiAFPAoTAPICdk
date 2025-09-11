using System.Text.Json.Serialization;

namespace QueTalMiAFPAoTAPI.Models {
    public class ParametroNotificacion {
        [JsonPropertyName("IdTipoNotificacion")]
        public required short IdTipoNotificacion { get; set; }
    }
}
