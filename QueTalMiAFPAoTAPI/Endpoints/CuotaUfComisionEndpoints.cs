using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class CuotaUfComisionEndpoints {
        public static IEndpointRouteBuilder MapCuotaUfComisionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder cuotaUfComisionGroup = routes.MapGroup("/CuotaUfComision");
            cuotaUfComisionGroup.MapUltimaFechaAlgunaEndpoint();
            cuotaUfComisionGroup.MapUltimaFechaTodasEndpoint();
            cuotaUfComisionGroup.MapObtenerCuotasEndpoint();
            cuotaUfComisionGroup.MapObtenerUltimaCuotaEndpoint();
            cuotaUfComisionGroup.MapObtenerRentabilidadRealEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapUltimaFechaAlgunaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/UltimaFechaAlguna", async (CuotaUfComisionDAO cuotaUfComisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    DateTime ultimaFecha = await cuotaUfComisionDAO.ObtenerUltimaFechaAlguna();

                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaAlguna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo la última fecha con algún valor cuota exitosamente.");

                    return Results.Ok(ultimaFecha);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaAlguna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener la última fecha con algún valor cuota. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }

        private static IEndpointRouteBuilder MapUltimaFechaTodasEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/UltimaFechaTodas", async (CuotaUfComisionDAO cuotaUfComisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    DateTime ultimaFecha = await cuotaUfComisionDAO.ObtenerUltimaFechaTodas();

                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo la última fecha con todos los valores cuota exitosamente.");

                    return Results.Ok(ultimaFecha);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener la última fecha con todos los valores cuota. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerCuotasEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerCuotas", async (string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal, CuotaUfComisionDAO cuotaUfComisionDAO, S3Helper s3Helper, VariableEntorno variableEntorno) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string[] afps = listaAFPs.ToUpper().Replace(" ", "").Split(",");
                    string[] fondos = listaFondos.ToUpper().Replace(" ", "").Split(",");
                    string[] diaMesAnnoInicio = fechaInicial.Split("/");
                    string[] diaMesAnnoFinal = fechaFinal.Split("/");

                    DateTime dtFechaInicio = new(
                        int.Parse(diaMesAnnoInicio[2]),
                        int.Parse(diaMesAnnoInicio[1]),
                        int.Parse(diaMesAnnoInicio[0])
                    );
                    DateTime dtFechaFinal = new(
                        int.Parse(diaMesAnnoFinal[2]),
                        int.Parse(diaMesAnnoFinal[1]),
                        int.Parse(diaMesAnnoFinal[0])
                    );

                    List<CuotaUf> cuotas = await cuotaUfComisionDAO.ObtenerCuotas(afps, fondos, dtFechaInicio, dtFechaFinal);

                    string jsonRetorno = JsonSerializer.Serialize(cuotas, AppJsonSerializerContext.Default.ListCuotaUf);
                    int cantBytes = Encoding.UTF8.GetByteCount(jsonRetorno);

                    SalObtenerCuotas? retorno = null;
                    if (cantBytes > 5 * 1000 * 1000) {
                        string s3url = await s3Helper.UploadFile(
                            variableEntorno.Obtener("BUCKET_NAME_LARGE_RESPONSES"),
                            Guid.NewGuid().ToString(),
                            jsonRetorno
                        );
                        retorno = new SalObtenerCuotas(s3url, null);
                    } else {
                        retorno = new SalObtenerCuotas(null, cuotas);
                    }

                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [ObtenerCuotas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo las cuotas exitosamente - AFP {listaAFPs} - Fondos {listaFondos} - Fecha Inicial {fechaInicial:yyyy-MM-dd} - Fecha Final {fechaFinal:yyyy-MM-dd}: " +
                        $"S3 URL {retorno.S3Url} - {retorno.ListaCuotas?.Count} registros.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [ObtenerCuotas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener las cuotas - AFP {listaAFPs} - Fondos {listaFondos} - Fecha Inicial {fechaInicial:yyyy-MM-dd} - Fecha Final {fechaFinal:yyyy-MM-dd}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerUltimaCuotaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ObtenerUltimaCuota", async (EntObtenerUltimaCuota entrada, CuotaUfComisionDAO cuotaUfComisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string[] afps = entrada.ListaAFPs.ToUpper().Replace(" ", "").Split(",");
                    string[] fondos = entrada.ListaFondos.ToUpper().Replace(" ", "").Split(",");
                    DateTime[] fechas = [.. entrada.ListaFechas.Replace(" ", "").Split(",").Select(f => {
                        string[] diaMesAnno = f.Split("/");
                        return new DateTime(
                            int.Parse(diaMesAnno[2]),
                            int.Parse(diaMesAnno[1]),
                            int.Parse(diaMesAnno[0])
                        );
                    })];

                    List<CuotaUfComision> cuotas = await cuotaUfComisionDAO.ObtenerUltimaCuota(afps, fondos, fechas);

                    List<SalObtenerUltimaCuota> retorno = [];
                    foreach (string afp in afps) {
                        foreach (string fondo in fondos) {
                            foreach (DateTime fecha in fechas) {
                                CuotaUfComision? cuota = cuotas.Where(c => c.Afp == afp && c.Fondo == fondo && c.Fecha <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();

                                if (cuota != null) {
                                    retorno.Add(new SalObtenerUltimaCuota(
                                        cuota.Afp,
                                        cuota.Fecha,
                                        cuota.Fondo,
                                        cuota.Valor,
                                        entrada.TipoComision == (byte)TipoComision.DeposCotizOblig ? cuota.ComisDeposCotizOblig : cuota.ComisAdminCtaAhoVol
                                    ));
                                }
                            }
                        }
                    }

                    LambdaLogger.Log(
                        $"[POST] - [CuotaUfComision] - [ObtenerUltimaCuota] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo la última cuota exitosamente - AFP {entrada.ListaAFPs} - Fondos {entrada.ListaFondos} - Fechas {entrada.ListaFechas}: {retorno.Count} registros.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [CuotaUfComision] - [ObtenerUltimaCuota] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener la última cuota - AFP {entrada.ListaAFPs} - Fondos {entrada.ListaFondos} - Fechas {entrada.ListaFechas}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            
            return routes;
        }
    
        private static IEndpointRouteBuilder MapObtenerRentabilidadRealEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerRentabilidadReal", async (string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal, CuotaUfComisionDAO cuotaUfComisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string[] afps = listaAFPs.ToUpper().Replace(" ", "").Split(",");
                    string[] fondos = listaFondos.ToUpper().Replace(" ", "").Split(",");

                    List<RentabilidadReal> retorno = [];

                    List<CuotaUfComision> cuotas = await cuotaUfComisionDAO.ObtenerUltimaCuota(afps, fondos, [ fechaFinal, fechaInicial ]);

                    foreach (string afp in afps) {
                        foreach (string fondo in fondos) {
                            CuotaUfComision? cuotaInicial = cuotas.Where(c => c.Afp == afp && c.Fondo == fondo).OrderBy(c => c.Fecha).FirstOrDefault();
                            CuotaUfComision? cuotaFinal = cuotas.Where(c => c.Afp == afp && c.Fondo == fondo).OrderByDescending(c => c.Fecha).FirstOrDefault();

                            if (cuotaInicial?.ValorUf != null && cuotaFinal?.ValorUf != null) {
                                retorno.Add(new RentabilidadReal(
                                    cuotaFinal.Afp,
                                    cuotaFinal.Fondo,
                                    cuotaInicial.Valor,
                                    cuotaInicial.ValorUf.Value,
                                    cuotaFinal.Valor,
                                    cuotaFinal.ValorUf.Value,
                                    (cuotaFinal.Valor * cuotaInicial.ValorUf.Value / (cuotaInicial.Valor * cuotaFinal.ValorUf.Value) - 1) * 100
                                ));
                            }
                        }
                    }

                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [ObtenerRentabilidadReal] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo la rentabilidad real exitosamente - AFP {listaAFPs} - Fondos {listaFondos} - Fecha Inicial {fechaInicial:yyyy-MM-dd} - Fecha Final {fechaFinal:yyyy-MM-dd}: {retorno.Count} registros.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [ObtenerRentabilidadReal] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener la rentabilidad real - AFP {listaAFPs} - Fondos {listaFondos} - Fecha Inicial {fechaInicial:yyyy-MM-dd} - Fecha Final {fechaFinal:yyyy-MM-dd}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
