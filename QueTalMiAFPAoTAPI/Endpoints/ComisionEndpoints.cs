using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class ComisionEndpoints {

        public static IEndpointRouteBuilder MapComisionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Comision");
            group.MapActualizacionMasivaEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapActualizacionMasivaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaComision comisionesExtraidas, ComisionDAO comisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    SalActualizacionMasivaComision retorno = new() {
                        CantComisionesInsertadas = 0,
                        CantComisionesActualizadas = 0
                    };

                    foreach (Comision comision in comisionesExtraidas.Comisiones) {
                        Comision? comisionExistente = await comisionDAO.ObtenerComision(comision.TipoComision, comision.Afp, comision.Fecha);

                        if (comisionExistente == null) {
                            await comisionDAO.InsertarComision(comision);
                            retorno.CantComisionesInsertadas++;
                        } else if (comisionExistente.Valor != comision.Valor || comisionExistente.TipoValor != comision.TipoValor) {
                            comisionExistente.Valor = comision.Valor;
                            comisionExistente.TipoValor = comision.TipoValor;
                            await comisionDAO.ActualizarComision(comisionExistente);
                            retorno.CantComisionesActualizadas++;
                        }
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Comision] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de comisiones exitosa: {retorno.CantComisionesInsertadas} insertadas y {retorno.CantComisionesActualizadas} actualizadas.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Comision] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización masiva de comisiones. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
