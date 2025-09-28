using Amazon.DynamoDBv2.Model;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Entities.DynamoDB {
    public class Cuota : Base {
        public required string Afp { get; set; }
        public required string Fondo { get; set; }
        public required DateOnly Fecha { get; set; }
        public required decimal Valor { get; set; }
        public DateTimeOffset? FechaCreacion { get; set; }
        public DateTimeOffset? FechaModificacion { get; set; }

        public override string PK => $"CUOTA#{Afp}#{Fondo}";

        public override string SK => $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        public override Dictionary<string, AttributeValue> ToItem() {
            Dictionary<string, AttributeValue> item = this.Key.Concat(new Dictionary<string, AttributeValue>() {
                { "Afp", new AttributeValue { S = $"{Afp}" } },
                { "Fondo", new AttributeValue { S = $"{Fondo}" } },
                { "Fecha", new AttributeValue { S = $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" } },
                { "Valor", new AttributeValue { N = $"{Valor.ToString(CultureInfo.InvariantCulture)}" } },
                { "FechaCreacion", new AttributeValue { NULL = true  } },
                { "FechaModificacion", new AttributeValue { NULL = true  } },
            }).ToDictionary();

            if (FechaCreacion != null) {
                item["FechaCreacion"] = new AttributeValue { S = $"{FechaCreacion.Value.ToString("o", CultureInfo.InvariantCulture)}" };
            }

            if (FechaModificacion != null) {
                item["FechaModificacion"] = new AttributeValue { S = $"{FechaModificacion.Value.ToString("o", CultureInfo.InvariantCulture)}" };
            }

            return item;
        }

        public static Cuota FromItem(Dictionary<string, AttributeValue> item) { 
            return new Cuota() { 
                Afp = item["Afp"].S,
                Fondo = item["Fondo"].S,
                Fecha = DateOnly.ParseExact(item["Fecha"].S, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Valor = decimal.Parse(item["Valor"].N, CultureInfo.InvariantCulture),
                FechaCreacion = item["FechaCreacion"].S != null ? DateTimeOffset.ParseExact(item["FechaCreacion"].S, "o", CultureInfo.InvariantCulture) : null,
                FechaModificacion = item["FechaModificacion"].S != null ? DateTimeOffset.ParseExact(item["FechaModificacion"].S, "o", CultureInfo.InvariantCulture) : null,
            };
        }

        public override bool Equals(object? obj) {
            if (obj is Cuota other) {
                return Afp == other.Afp && Fondo == other.Fondo && Fecha == other.Fecha;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Afp, Fondo, Fecha);
        }
    }
}
