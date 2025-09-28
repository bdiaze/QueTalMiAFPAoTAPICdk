using Amazon.DynamoDBv2.Model;

namespace QueTalMiAFPAoTAPI.Entities.DynamoDB {
    public abstract class Base {

        public abstract string PK { get; }
        public abstract string SK { get; }

        public Dictionary<string, AttributeValue> Key { 
            get {
                return new Dictionary<string, AttributeValue> {
                    { "PK", new AttributeValue() { S = PK } },
                    { "SK", new AttributeValue() { S = SK } }
                };
            } 
        }

        public abstract Dictionary<string, AttributeValue> ToItem();
    }
}
