using Npgsql;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class CuotaUfComisionDAO(DatabaseConnectionHelper connectionHelper) {

        public async Task<DateTime> ObtenerUltimaFechaAlguna() {
            DateTime ultimaFecha = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));

            using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            using NpgsqlCommand command = new();
            string queryString = "SELECT MAX(\"FECHA\") " +
                "FROM \"QueTalMiAFP\".\"CUOTA\";";

            command.CommandText = queryString;
            command.Connection = connection;

            await connection.OpenAsync();
            DbDataReader reader = await command.ExecuteReaderAsync();
            bool existe = await reader.ReadAsync();
            if (existe) {
                ultimaFecha = reader.GetDateTime(0);
            }
            await reader.CloseAsync();
            
            return ultimaFecha;
        }
    }
}
