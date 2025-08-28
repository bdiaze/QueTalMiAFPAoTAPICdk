namespace QueTalMiAFPAoTAPI.Models {
    public record SalObtenerCuotas(
        string? S3Url,
        List<CuotaUf>? ListaCuotas
    );
}
