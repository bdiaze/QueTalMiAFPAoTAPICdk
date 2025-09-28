using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using QueTalMiAFPAoTAPI.Entities.DynamoDB;
using QueTalMiAFPAoTAPI.Helpers;
using System.Linq;

namespace QueTalMiAFPAoTAPI.Repositories.DynamoDB {
    public class UfDAO(VariableEntornoHelper variableEntorno, IAmazonDynamoDB dynamoDB) {
        private readonly string TABLE_NAME = variableEntorno.Obtener("NAME_DYNAMODB_SINGLE_TABLE");

        public async Task<Uf?> ObtenerUna(DateOnly fecha, bool consistentRead = false) {
            GetItemRequest request = new() {
                TableName = TABLE_NAME,
                Key = new Uf() { 
                    Fecha = fecha, 
                    Valor = 0 
                }.Key,
                ConsistentRead = consistentRead
            };

            GetItemResponse response = await dynamoDB.GetItemAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al obtener la UF de DynamoDB - StatusCode: {response.HttpStatusCode}");
            }

            return Uf.FromItem(response.Item);
        }

        public async Task<Dictionary<DateOnly, Uf>> ObtenerVarias(HashSet<DateOnly> fechas, bool consistentRead = false) {
            Dictionary<DateOnly, Uf> salida = [];

            byte chunkSize = 100;

            List<DateOnly> listaFechas = [.. fechas];
            for (int i = 0; i < listaFechas.Count; i += chunkSize) {
                List<DateOnly> chunk = [.. listaFechas.Skip(i).Take(chunkSize)];

                BatchGetItemRequest request = new() {
                    RequestItems = new Dictionary<string, KeysAndAttributes> {
                        [TABLE_NAME] = new KeysAndAttributes {
                            Keys = [.. chunk.Select(f => new Uf() { Fecha = f, Valor = 0 }.Key)],
                            ConsistentRead = consistentRead,
                        }
                    }
                };

                BatchGetItemResponse response;
                do {
                    response = await dynamoDB.BatchGetItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al obtener las UF de DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    if (response.Responses.TryGetValue(TABLE_NAME, out List<Dictionary<string, AttributeValue>>? items)) {
                        if (items != null) {
                            foreach (Uf uf in items.Select(Uf.FromItem)) {
                                salida[uf.Fecha] = uf;
                            }
                        }
                    }

                    request.RequestItems = response.UnprocessedKeys;
                } while (response.UnprocessedKeys.Count > 0);
            }

            return salida;
        }

        public async Task InsertarUna(Uf uf) {
            PutItemRequest request = new() {
                TableName = TABLE_NAME,
                Item = uf.ToItem(),
                ConditionExpression = "attribute_not_exists(PK)"
            };

            PutItemResponse response = await dynamoDB.PutItemAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al insertar la UF en DynamoDB - StatusCode: {response.HttpStatusCode}");
            }
        }

        public async Task ActualizarUna(Uf uf) {
            PutItemRequest request = new() {
                TableName = TABLE_NAME,
                Item = uf.ToItem(),
                ConditionExpression = "attribute_exists(PK)"
            };

            PutItemResponse response = await dynamoDB.PutItemAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al actualizar la UF en DynamoDB - StatusCode: {response.HttpStatusCode}");
            }
        }

        public async Task InsertarOActualizarVarias(HashSet<Uf> ufs) {
            byte chunkSize = 25;

            List<Uf> listaUfs = [.. ufs];
            for (int i = 0; i < listaUfs.Count; i += chunkSize) {
                List<Uf> chunk = [.. listaUfs.Skip(i).Take(chunkSize)];

                BatchWriteItemRequest request = new() {
                    RequestItems = new Dictionary<string, List<WriteRequest>> {
                        [TABLE_NAME] = [.. chunk.Select(f => new WriteRequest { PutRequest = new PutRequest { Item = f.ToItem() } })],
                    }
                };

                BatchWriteItemResponse response;
                do {
                    response = await dynamoDB.BatchWriteItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception($"Ocurrió un error al insertar las UF en DynamoDB - StatusCode: {response.HttpStatusCode}");
                    }

                    request.RequestItems = response.UnprocessedItems;
                } while (response.UnprocessedItems.Count > 0);
            }
        }
    }
}
