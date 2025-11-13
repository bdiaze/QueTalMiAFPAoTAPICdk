using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class MensajeUsuarioDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<MensajeUsuario>> ObtenerMensajesUsuarios(DateTimeOffset fechaDesde, DateTimeOffset fechaHasta) {
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

            command.Parameters.AddWithValue("@FechaDesde", fechaDesde.ToUniversalTime());
            command.Parameters.AddWithValue("@FechaHasta", fechaHasta.ToUniversalTime());

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                mensajes.Add(new MensajeUsuario { 
                    IdMensaje = reader.GetInt64(0),
                    IdTipoMensaje = reader.GetInt16(1),
                    FechaIngreso = reader.GetDateTime(2),
                    Nombre = reader.GetString(3),
                    Correo = reader.GetString(4),
                    Mensaje = reader.GetString(5),
                    TipoMensaje = new TipoMensaje {
                        IdTipoMensaje = reader.GetInt16(1),
                        DescripcionCorta = reader.GetString(6),
                        DescripcionLarga = reader.GetString(7),
                        Vigencia = reader.GetByte(8)
                    }
                });
            }
            await reader.CloseAsync();

            return mensajes;
        }

        public async Task<MensajeUsuario> IngresarMensajeUsuario(short idTipoMensaje, DateTimeOffset fechaIngreso, string nombre, string correo, string mensaje) {
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
            command.Parameters.AddWithValue("@FechaIngreso", fechaIngreso.ToUniversalTime());
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@Correo", correo);
            command.Parameters.AddWithValue("@Mensaje", mensaje);

            long idMensaje = (long)(await command.ExecuteScalarAsync())!;

            return new MensajeUsuario {
                IdMensaje = idMensaje,
                IdTipoMensaje = idTipoMensaje,
                FechaIngreso = fechaIngreso,
                Nombre = nombre,
                Correo = correo,
                Mensaje = mensaje,
            };
        }
    }
}
