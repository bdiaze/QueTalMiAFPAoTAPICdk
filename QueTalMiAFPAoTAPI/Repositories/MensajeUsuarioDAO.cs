using Npgsql;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class MensajeUsuarioDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<MensajeUsuario>> ObtenerMensajesUsuarios(DateTime fechaDesde, DateTime fechaHasta) {
            List<MensajeUsuario> mensajes = [];

            string queryString = "SELECT MU.\"ID_MENSAJE\", MU.\"ID_TIPO_MENSAJE\", MU.\"FECHA_INGRESO\", " +
                    "MU.\"NOMBRE\", MU.\"CORREO\", MU.\"MENSAJE\", TM.\"DESCRIPCION_CORTA\", TM.\"DESCRIPCION_LARGA\", TM.\"VIGENCIA\" " +
                    "FROM \"QueTalMiAFP\".\"MENSAJE_USUARIO\" MU " +
                    "INNER JOIN \"QueTalMiAFP\".\"TIPO_MENSAJE\" TM " +
                    "ON TM.\"ID_TIPO_MENSAJE\" = MU.\"ID_TIPO_MENSAJE\" " +
                    "WHERE MU.\"FECHA_INGRESO\" >= @FechaDesde " +
                    "AND MU.\"FECHA_INGRESO\" <= @FechaHasta " +
                    "ORDER BY MU.\"FECHA_INGRESO\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@FechaDesde", fechaDesde);
            command.Parameters.AddWithValue("@FechaHasta", fechaHasta);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                mensajes.Add(new MensajeUsuario(
                    reader.GetInt64(0),
                    reader.GetInt16(1),
                    reader.GetDateTime(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    new TipoMensaje(
                        reader.GetInt16(1),
                        reader.GetString(6),
                        reader.GetString(7),
                        reader.GetByte(8),
                        null
                    )
                ));
            }
            await reader.CloseAsync();

            return mensajes;
        }

        public async Task<MensajeUsuario> IngresarMensajeUsuario(short idTipoMensaje, DateTime fechaIngreso, string nombre, string correo, string mensaje) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"MENSAJE_USUARIO\"(" +
                "\"ID_TIPO_MENSAJE\", " +
                "\"FECHA_INGRESO\", " +
                "\"NOMBRE\", " +
                "\"CORREO\", " +
                "\"MENSAJE\"" +
                ") VALUES (" +
                "@IdTipoMensaje, " +
                "@FechaIngreso, " +
                "@Nombre, " +
                "@Correo, " +
                "@Mensaje" +
                ") " +
                "RETURNING \"ID_MENSAJE\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdTipoMensaje", idTipoMensaje);
            command.Parameters.AddWithValue("@FechaIngreso", fechaIngreso);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@Correo", correo);
            command.Parameters.AddWithValue("@Mensaje", mensaje);

            long idMensaje = (long)(await command.ExecuteScalarAsync())!;

            return new MensajeUsuario(
                idMensaje,
                idTipoMensaje,
                fechaIngreso,
                nombre,
                correo,
                mensaje,
                null
            );
        }
    }
}
