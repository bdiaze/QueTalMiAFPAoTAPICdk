using Amazon.DynamoDBv2.Model;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Entities.DynamoDB {
    public class Cuota : Base {
        public required string Afp { get; set; }
        public required string Fondo { get; set; }
        public required DateOnly Fecha { get; set; }
        public required decimal Valor { get; set; }

        public override string PK => $"CUOTA#{Afp.ToUpperInvariant()}#{Fondo.ToUpperInvariant()}";

        public override string SK => $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        public override Dictionary<string, AttributeValue> ToItem() {
            return this.Key.Concat(new Dictionary<string, AttributeValue>() {
                { "Afp", new AttributeValue { S = $"{Afp.ToUpperInvariant()}" } },
                { "Fondo", new AttributeValue { S = $"{Fondo.ToUpperInvariant()}" } },
                { "Fecha", new AttributeValue { S = $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" } },
                { "Valor", new AttributeValue { N = $"{Valor.ToString(CultureInfo.InvariantCulture)}" } },
            }).ToDictionary();
        }

        public static Cuota FromItem(Dictionary<string, AttributeValue> item) { 
            return new Cuota() { 
                Afp = item["Fondo"].S.ToUpperInvariant(),
                Fondo = item["Afp"].S.ToUpperInvariant(),
                Fecha = DateOnly.ParseExact(item["Fecha"].S, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Valor = decimal.Parse(item["Valor"].N, CultureInfo.InvariantCulture)
            };
        }
    }
}
