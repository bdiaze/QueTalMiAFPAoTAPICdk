using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data.Common;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class CuotaUfComisionDAO(DatabaseConnectionHelper connectionHelper) {

        public async Task<DateTime> ObtenerUltimaFechaAlguna() {
            DateTime ultimaFecha = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));

            string queryString = "SELECT MAX(\"FECHA\") " +
                "FROM \"QueTalMiAFP\".\"CUOTA\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);
            DbDataReader reader = await command.ExecuteReaderAsync();
            bool existe = await reader.ReadAsync();
            if (existe) {
                ultimaFecha = reader.GetDateTime(0);
            }
            await reader.CloseAsync();
            
            return ultimaFecha;
        }

        public async Task<DateTime> ObtenerUltimaFechaTodas() {
            DateTime ultimaFecha = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));

            string queryString = "SELECT MIN(TMP.\"MAX_FECHA\") " +
                    "FROM ( " +
                    "SELECT \"AFP\", \"FONDO\", MAX(\"FECHA\") AS \"MAX_FECHA\" " +
                    "FROM \"QueTalMiAFP\".\"CUOTA\" " +
                    "GROUP BY \"AFP\", \"FONDO\" " +
                    ") TMP;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);
            DbDataReader reader = await command.ExecuteReaderAsync();
            bool existe = await reader.ReadAsync();
            if (existe) {
                ultimaFecha = reader.GetDateTime(0);
            }
            await reader.CloseAsync();

            return ultimaFecha;
        }

        public async Task<List<CuotaUf>> ObtenerCuotas(string[] afps, string[] fondos, DateTime dtFechaInicio, DateTime dtFechaFinal) {
            List<CuotaUf> cuotas = [];

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new();

            string[] parametrosAfp = new string[afps.Length];
            for (int i = 0; i < afps.Length; i++) {
                parametrosAfp[i] = "@Afp" + i;
                command.Parameters.AddWithValue(parametrosAfp[i], afps[i]);
            }

            string[] parametrosFondo = new string[fondos.Length];
            for (int i = 0; i < fondos.Length; i++) {
                parametrosFondo[i] = "@Fondo" + i;
                command.Parameters.AddWithValue(parametrosFondo[i], fondos[i]);
            }
            
            command.Parameters.AddWithValue("@FechaInicial", dtFechaInicio);
            command.Parameters.AddWithValue("@FechaFinal", dtFechaFinal);

            string queryString = string.Format("SELECT \"AFP\", \"FECHA\", \"FONDO\", \"VALOR\", \"VALOR_UF\" " +
                "FROM \"QueTalMiAFP\".\"CUOTA_UF_COMISION\" " +
                "WHERE \"AFP\" IN ({0}) " +
                "AND \"FONDO\" IN ({1}) " +
                "AND \"FECHA\" >= @FechaInicial " +
                "AND \"FECHA\" <= @FechaFinal " +
                "AND \"VALOR_UF\" IS NOT NULL " +
                "ORDER BY \"FECHA\", \"FONDO\", \"AFP\";",
                string.Join(", ", parametrosAfp),
                string.Join(", ", parametrosFondo)
            );

            command.CommandText = queryString;
            command.Connection = connection;

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                cuotas.Add(new CuotaUf { 
                    Afp = reader.GetString(0),
                    Fecha = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                    Fondo = reader.GetString(2),
                    Valor = Math.Round(reader.GetDecimal(3), 2),
                    ValorUf = await reader.IsDBNullAsync(4) ? null : Math.Round(reader.GetDecimal(4), 2)
                });
            }
            await reader.CloseAsync();
            
            return cuotas;
        }

        public async Task<Dictionary<string, Dictionary<string, SortedDictionary<DateTime, CuotaUfComision>>>> ObtenerUltimaCuota(string[] afps, string[] fondos, DateTime[] fechas) {
            Dictionary<string, Dictionary<string, SortedDictionary<DateTime, CuotaUfComision>>> cuotas = [];

            string queryString = "SELECT CUC.\"AFP\", CUC.\"FECHA\", CUC.\"FONDO\", CUC.\"VALOR\", CUC.\"VALOR_UF\", " +
                "CUC.\"COMIS_DEPOS_COTIZ_OBLIG\", CUC.\"COMIS_ADMIN_CTA_AHO_VOL\" " +
                "FROM \"QueTalMiAFP\".\"CUOTA_UF_COMISION\" CUC " +
                "WHERE (CUC.\"AFP\", CUC.\"FONDO\", CUC.\"FECHA\") IN (SELECT \"STT_AFP\" AS \"AFP\", \"STT_FONDO\" AS \"FONDO\", " +
                "(SELECT MAX(CU.\"FECHA\") " +
                "FROM \"QueTalMiAFP\".\"CUOTA\" CU " +
                "WHERE CU.\"AFP\" = \"STT_AFP\" " +
                "AND CU.\"FONDO\" = \"STT_FONDO\" " +
                "AND CU.\"FECHA\" <= TO_DATE(\"STT_FECHA\", 'YYYY-MM-DD')) AS \"FECHA\" " +
                "FROM STRING_TO_TABLE(@Afps, ',') AS \"STT_AFP\" " +
                "CROSS JOIN STRING_TO_TABLE(@Fondos, ',') AS \"STT_FONDO\" " +
                "CROSS JOIN STRING_TO_TABLE(@Fechas, ',') AS \"STT_FECHA\");";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afps", string.Join(",", afps));
            command.Parameters.AddWithValue("@Fondos", string.Join(",", fondos));
            command.Parameters.AddWithValue("@Fechas", string.Join(",", fechas.Select(f => f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                CuotaUfComision cuota = new() { 
                    Afp = reader.GetString(0),
                    Fecha = reader.GetDateTime(1),
                    Fondo = reader.GetString(2),
                    Valor = Math.Round(reader.GetDecimal(3), 2),
                    ValorUf = await reader.IsDBNullAsync(4) ? null : Math.Round(reader.GetDecimal(4), 2),
                    ComisDeposCotizOblig = await reader.IsDBNullAsync(5) ? null : Math.Round(reader.GetDecimal(5), 2),
                    ComisAdminCtaAhoVol = await reader.IsDBNullAsync(6) ? null : Math.Round(reader.GetDecimal(6), 2)
                };

                if (!cuotas.TryGetValue(cuota.Afp, out Dictionary<string, SortedDictionary<DateTime, CuotaUfComision>>? dictFondos)) {
                    dictFondos = [];
                    cuotas.Add(cuota.Afp, dictFondos);
                }

                if (!dictFondos.TryGetValue(cuota.Fondo, out SortedDictionary<DateTime, CuotaUfComision>? dictCuotas)) {
                    dictCuotas = [];
                    dictFondos.Add(cuota.Fondo, dictCuotas);
                }

                dictCuotas.Add(cuota.Fecha, cuota);
            }
            await reader.CloseAsync();
            
            return cuotas;
        }

    }
}
