using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using System.Text;

namespace QueTalMiAFPAoTAPI.Helpers {
    public class S3Helper(IAmazonS3 client) {
        public async Task<string> UploadFile(string bucketName, string keyName, string contenido) {
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream, Encoding.UTF8);
            writer.Write(contenido);
            writer.Flush();
            stream.Position = 0;

            PutObjectResponse response = await client.PutObjectAsync(new PutObjectRequest {
                BucketName = bucketName,
                Key = keyName,
                InputStream = stream
            });

            if (response.HttpStatusCode != HttpStatusCode.OK) {
                throw new Exception("No se pudo subir correctamente el objeto S3");
            }

            return keyName;
        }
    }
}
