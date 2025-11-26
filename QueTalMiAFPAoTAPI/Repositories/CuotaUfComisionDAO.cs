using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class CuotaUfComisionDAO(DatabaseConnectionHelper connectionHelper) {

        public async Task<DateOnly> ObtenerUltimaFechaAlguna() {
			DateOnly ultimaFecha = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time")));

            string queryString = "SELECT MAX(\"FECHA\") " +
                "FROM \"QueTalMiAFP\".\"CUOTA\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);
            DbDataReader reader = await command.ExecuteReaderAsync();
            bool existe = await reader.ReadAsync();
            if (existe) {
                ultimaFecha = DateOnly.FromDateTime(reader.GetDateTime(0));
            }
            await reader.CloseAsync();
            
            return ultimaFecha;
        }

        public async Task<DateOnly> ObtenerUltimaFechaTodas() {
			DateOnly ultimaFecha = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time")));

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
                ultimaFecha = DateOnly.FromDateTime(reader.GetDateTime(0));
            }
            await reader.CloseAsync();

            return ultimaFecha;
        }

        public async Task<List<CuotaUf>> ObtenerCuotas(string[] afps, string[] fondos, DateOnly dtFechaInicio, DateOnly dtFechaFinal) {
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
                    Fecha = DateOnly.FromDateTime(reader.GetDateTime(1)),
                    Fondo = reader.GetString(2),
                    Valor = Math.Round(reader.GetDecimal(3), 2),
                    ValorUf = await reader.IsDBNullAsync(4) ? null : Math.Round(reader.GetDecimal(4), 2)
                });
            }
            await reader.CloseAsync();
            
            return cuotas;
        }

        public async Task<Dictionary<string, Dictionary<string, SortedDictionary<DateOnly, CuotaUfComision>>>> ObtenerUltimaCuota(string[] afps, string[] fondos, DateOnly[] fechas) {
            Dictionary<string, Dictionary<string, SortedDictionary<DateOnly, CuotaUfComision>>> cuotas = [];
                                         
			string queryString = "WITH PARAMS AS (" +
				"SELECT STT_AFP AS AFP, STT_FONDO AS FONDO, TO_DATE(STT_FECHA, 'YYYY-MM-DD') AS FECHA_LIMITE " +
				"FROM STRING_TO_TABLE(@Afps, ',') AS STT_AFP " +
				"CROSS JOIN STRING_TO_TABLE(@Fondos, ',') AS STT_FONDO " +
				"CROSS JOIN STRING_TO_TABLE(@Fechas, ',') AS STT_FECHA), " +
				"CUOTAS AS (" +
				"SELECT TMP.\"AFP\", TMP.\"FONDO\", TMP.\"FECHA\", TMP.\"VALOR\" " +
				"FROM PARAMS P " +
				"CROSS JOIN LATERAL (" +
				"SELECT CU.\"AFP\", CU.\"FONDO\", CU.\"FECHA\", CU.\"VALOR\" " +
				"FROM \"QueTalMiAFP\".\"CUOTA\" CU " +
				"WHERE CU.\"AFP\" = P.AFP " +
				"AND CU.\"FONDO\" = P.FONDO " +
				"AND CU.\"FECHA\" <= P.FECHA_LIMITE " +
				"ORDER BY CU.\"FECHA\" DESC " +
				"LIMIT 1) TMP) " +
				"SELECT CU.\"AFP\", CU.\"FECHA\", CU.\"FONDO\", CU.\"VALOR\", UF.\"VALOR\" AS \"VALOR_UF\", " +
				"CO1.\"VALOR\" AS \"COMIS_DEPOS_COTIZ_OBLIG\", CO2.\"VALOR\" AS \"COMIS_ADMIN_CTA_AHO_VOL\" " +
				"FROM CUOTAS CU " +
				"LEFT JOIN \"QueTalMiAFP\".\"UF\" UF " +
				"ON UF.\"FECHA\" = CU.\"FECHA\" " +
				"LEFT JOIN \"QueTalMiAFP\".\"COMISION\" CO1 " +
				"ON CO1.\"AFP\" = CU.\"AFP\" AND " +
				"CO1.\"FECHA\" = DATE_TRUNC('MONTH', CU.\"FECHA\") AND " +
				"CO1.\"TIPO_COMISION\" = 1 " +
				"LEFT JOIN \"QueTalMiAFP\".\"COMISION\" CO2 " +
				"ON CO2.\"AFP\" = CU.\"AFP\" AND " +
				"CO2.\"FECHA\" = DATE_TRUNC('MONTH', CU.\"FECHA\") AND " +
				"CO2.\"TIPO_COMISION\" = 2;";

			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afps", string.Join(",", afps));
            command.Parameters.AddWithValue("@Fondos", string.Join(",", fondos));
            command.Parameters.AddWithValue("@Fechas", string.Join(",", fechas.Select(f => f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                CuotaUfComision cuota = new() { 
                    Afp = reader.GetString(0),
                    Fecha = DateOnly.FromDateTime(reader.GetDateTime(1)),
                    Fondo = reader.GetString(2),
                    Valor = Math.Round(reader.GetDecimal(3), 2),
                    ValorUf = await reader.IsDBNullAsync(4) ? null : Math.Round(reader.GetDecimal(4), 2),
                    ComisDeposCotizOblig = await reader.IsDBNullAsync(5) ? null : Math.Round(reader.GetDecimal(5), 2),
                    ComisAdminCtaAhoVol = await reader.IsDBNullAsync(6) ? null : Math.Round(reader.GetDecimal(6), 2)
                };

                if (!cuotas.TryGetValue(cuota.Afp, out Dictionary<string, SortedDictionary<DateOnly, CuotaUfComision>>? dictFondos)) {
                    dictFondos = [];
                    cuotas.Add(cuota.Afp, dictFondos);
                }

                if (!dictFondos.TryGetValue(cuota.Fondo, out SortedDictionary<DateOnly, CuotaUfComision>? dictCuotas)) {
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
