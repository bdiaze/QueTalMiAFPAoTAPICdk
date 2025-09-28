using Amazon.DynamoDBv2.Model;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Entities.DynamoDB {
    public class Uf : Base {
        public required DateOnly Fecha { get; set; }
        public required decimal Valor { get; set; }

        public override string PK => $"UF";

        public override string SK => $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        public override Dictionary<string, AttributeValue> ToItem() {
            return this.Key.Concat(new Dictionary<string, AttributeValue>() {
                { "Fecha", new AttributeValue { S = $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" } },
                { "Valor", new AttributeValue { N = $"{Valor.ToString(CultureInfo.InvariantCulture)}" } },
            }).ToDictionary();
        }

        public static Uf FromItem(Dictionary<string, AttributeValue> item) {
            return new Uf() {
                Fecha = DateOnly.ParseExact(item["Fecha"].S, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Valor = decimal.Parse(item["Valor"].N, CultureInfo.InvariantCulture),
            };
        }
    }
}
