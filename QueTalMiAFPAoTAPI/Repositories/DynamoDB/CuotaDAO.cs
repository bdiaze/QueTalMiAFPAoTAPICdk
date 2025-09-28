using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using QueTalMiAFPAoTAPI.Entities.DynamoDB;
using QueTalMiAFPAoTAPI.Helpers;

namespace QueTalMiAFPAoTAPI.Repositories.DynamoDB {
    public class CuotaDAO(VariableEntornoHelper variableEntorno, IAmazonDynamoDB dynamoDB) {
        private readonly string TABLE_NAME = variableEntorno.Obtener("NAME_DYNAMODB_SINGLE_TABLE");

        public async Task<Dictionary<string, Dictionary<string, Dictionary<DateOnly, Cuota>>>> ObtenerVarias(HashSet<(string afp, string fondo, DateOnly fecha)> cuotas, bool consistentRead = false) {
            Dictionary<string, Dictionary<string, Dictionary<DateOnly, Cuota>>> salida = [];

            byte chunkSize = 100;

            List<(string afp, string fondo, DateOnly fecha)> listaCuotas = [.. cuotas];
            for (int i = 0; i < listaCuotas.Count; i += chunkSize) {
                List<(string afp, string fondo, DateOnly fecha)> chunk = [.. listaCuotas.Skip(i).Take(chunkSize)];

                BatchGetItemRequest request = new() {
                    RequestItems = new Dictionary<string, KeysAndAttributes> {
                        [TABLE_NAME] = new KeysAndAttributes {
                            Keys = [.. chunk.Select(f => new Cuota() { Afp = f.afp, Fondo = f.fondo, Fecha = f.fecha, Valor = 0 }.Key)],
                            ConsistentRead = consistentRead,
                        }
                    }
                };

                BatchGetItemResponse response;
                do {
                    response = await dynamoDB.BatchGetItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al obtener las cuotas de DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    if (response.Responses.TryGetValue(TABLE_NAME, out List<Dictionary<string, AttributeValue>>? items)) {
                        if (items != null) {
                            foreach (Cuota cuota in items.Select(Cuota.FromItem)) {
                                if (salida.TryGetValue(cuota.Afp, out Dictionary<string, Dictionary<DateOnly, Cuota>>? dictFondos)) {
                                    if (dictFondos.TryGetValue(cuota.Fondo, out Dictionary<DateOnly, Cuota>? dictFechas)) {
                                        salida[cuota.Afp][cuota.Fondo][cuota.Fecha] = cuota;
                                    } else {
                                        salida[cuota.Afp][cuota.Fondo] = new Dictionary<DateOnly, Cuota>() {
                                            { cuota.Fecha, cuota}
                                        };
                                    }
                                } else {
                                    salida[cuota.Afp] = new Dictionary<string, Dictionary<DateOnly, Cuota>>() {
                                        { cuota.Fondo, new Dictionary<DateOnly, Cuota>{
                                            { cuota.Fecha, cuota }
                                        } }
                                    };
                                }
                            }
                        }
                    }

                    request.RequestItems = response.UnprocessedKeys;
                } while (response.UnprocessedKeys.Count > 0);
            }

            return salida;
        }

        public async Task InsertarOActualizarVarias(HashSet<Cuota> cuotas) {
            byte chunkSize = 25;

            List<Cuota> listaCuotas = [.. cuotas];
            for (int i = 0; i < listaCuotas.Count; i += chunkSize) {
                List<Cuota> chunk = [.. listaCuotas.Skip(i).Take(chunkSize)];

                BatchWriteItemRequest request = new() {
                    RequestItems = new Dictionary<string, List<WriteRequest>> {
                        [TABLE_NAME] = [.. chunk.Select(f => new WriteRequest { PutRequest = new PutRequest { Item = f.ToItem() } })],
                    }
                };

                BatchWriteItemResponse response;
                do {
                    response = await dynamoDB.BatchWriteItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al insertar las cuotas en DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    request.RequestItems = response.UnprocessedItems;
                } while (response.UnprocessedItems.Count > 0);
            }
        }
    }
}
