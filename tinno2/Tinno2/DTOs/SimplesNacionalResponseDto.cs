using System.Text.Json.Serialization;

namespace Api.DTOs;

public class SimplesNacionalResponseDto
{
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("simples")]
    public SnSimplesDto? Simples { get; set; }

    [JsonPropertyName("simei")]
    public SnSimplesDto? Simei { get; set; }

    [JsonPropertyName("billing")]
    public BillingDto? Billing { get; set; }
}
