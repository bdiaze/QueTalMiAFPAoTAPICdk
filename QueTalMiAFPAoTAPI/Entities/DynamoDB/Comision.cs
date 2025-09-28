using Amazon.DynamoDBv2.Model;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Entities.DynamoDB {
    public class Comision : Base {
        public required string Afp { get; set; }
        public required DateOnly Fecha { get; set; }
        public required byte TipoComision { get; set; }
        public required byte TipoValor { get; set; }
        public required decimal Valor { get; set; }

        public override string PK => $"COMISION#{Afp.ToUpperInvariant()}#{TipoComision}";

        public override string SK => $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        public override Dictionary<string, AttributeValue> ToItem() {
            return this.Key.Concat(new Dictionary<string, AttributeValue>() {
                { "Afp", new AttributeValue { S = $"{Afp.ToUpperInvariant()}" } },
                { "Fecha", new AttributeValue { S = $"{Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" } },
                { "TipoComision", new AttributeValue { N = $"{TipoComision.ToString(CultureInfo.InvariantCulture)}" } },
                { "TipoValor", new AttributeValue { N = $"{TipoValor.ToString(CultureInfo.InvariantCulture)}" } },
                { "Valor", new AttributeValue { N = $"{Valor.ToString(CultureInfo.InvariantCulture)}" } },
            }).ToDictionary();
        }

        public static Comision FromItem(Dictionary<string, AttributeValue> item) {
            return new Comision() {
                Afp = item["Afp"].S.ToUpperInvariant(),
                Fecha = DateOnly.ParseExact(item["Fecha"].S, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                TipoComision = byte.Parse(item["TipoComision"].N, CultureInfo.InvariantCulture),
                TipoValor = byte.Parse(item["TipoValor"].N, CultureInfo.InvariantCulture),
                Valor = decimal.Parse(item["Valor"].N, CultureInfo.InvariantCulture),
            };
        }
    }
}
