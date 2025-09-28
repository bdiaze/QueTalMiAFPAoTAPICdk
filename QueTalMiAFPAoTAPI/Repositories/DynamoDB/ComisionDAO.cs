using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using QueTalMiAFPAoTAPI.Entities.DynamoDB;
using QueTalMiAFPAoTAPI.Helpers;

namespace QueTalMiAFPAoTAPI.Repositories.DynamoDB {
    public class ComisionDAO(VariableEntornoHelper variableEntorno, IAmazonDynamoDB dynamoDB) {
        private readonly string TABLE_NAME = variableEntorno.Obtener("NAME_DYNAMODB_SINGLE_TABLE");

        public async Task<Dictionary<string, Dictionary<byte, Dictionary<DateOnly, Comision>>>> ObtenerVarias(HashSet<(string afp, byte tipoComision, DateOnly fecha)> comisiones, bool consistentRead = false) {
            Dictionary<string, Dictionary<byte, Dictionary<DateOnly, Comision>>> salida = [];

            byte chunkSize = 100;

            List<(string afp, byte tipoComision, DateOnly fecha)> listaComisiones = [.. comisiones];
            for (int i = 0; i < listaComisiones.Count; i += chunkSize) {
                List<(string afp, byte tipoComision, DateOnly fecha)> chunk = [.. listaComisiones.Skip(i).Take(chunkSize)];

                BatchGetItemRequest request = new() {
                    RequestItems = new Dictionary<string, KeysAndAttributes> {
                        [TABLE_NAME] = new KeysAndAttributes {
                            Keys = [.. chunk.Select(f => new Comision() { Afp = f.afp, TipoComision = f.tipoComision, Fecha = f.fecha, TipoValor = 0, Valor = 0 }.Key)],
                            ConsistentRead = consistentRead,
                        }
                    }
                };

                BatchGetItemResponse response;
                do {
                    response = await dynamoDB.BatchGetItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al obtener las comisiones de DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    if (response.Responses.TryGetValue(TABLE_NAME, out List<Dictionary<string, AttributeValue>>? items)) {
                        if (items != null) {
                            foreach (Comision comision in items.Select(Comision.FromItem)) {
                                if (salida.TryGetValue(comision.Afp, out Dictionary<byte, Dictionary<DateOnly, Comision>>? dictTipoComisiones)) {
                                    if (dictTipoComisiones.TryGetValue(comision.TipoComision, out Dictionary<DateOnly, Comision>? dictFechas)) {
                                        salida[comision.Afp][comision.TipoComision][comision.Fecha] = comision;
                                    } else {
                                        salida[comision.Afp][comision.TipoComision] = new Dictionary<DateOnly, Comision>() {
                                            { comision.Fecha, comision}
                                        };
                                    }
                                } else {
                                    salida[comision.Afp] = new Dictionary<byte, Dictionary<DateOnly, Comision>>() {
                                        { comision.TipoComision, new Dictionary<DateOnly, Comision>{
                                            { comision.Fecha, comision }
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

        public async Task InsertarOActualizarVarias(HashSet<Comision> comisiones) {
            byte chunkSize = 25;

            List<Comision> listaComisiones = [.. comisiones];
            for (int i = 0; i < listaComisiones.Count; i += chunkSize) {
                List<Comision> chunk = [.. listaComisiones.Skip(i).Take(chunkSize)];

                BatchWriteItemRequest request = new() {
                    RequestItems = new Dictionary<string, List<WriteRequest>> {
                        [TABLE_NAME] = [.. chunk.Select(f => new WriteRequest { PutRequest = new PutRequest { Item = f.ToItem() } })],
                    }
                };

                BatchWriteItemResponse response;
                do {
                    response = await dynamoDB.BatchWriteItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al insertar las comisiones en DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    request.RequestItems = response.UnprocessedItems;
                } while (response.UnprocessedItems.Count > 0);
            }
        }
    }
}
