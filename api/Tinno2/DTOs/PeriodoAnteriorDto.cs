using System.Text.Json.Serialization;

namespace Api.DTOs;

public class PeriodoAnteriorDto
{
    [JsonPropertyName("inicio")]
    public string Inicio { get; set; } = string.Empty;

    [JsonPropertyName("fim")]
    public string Fim { get; set; } = string.Empty;

    [JsonPropertyName("detalhamento")]
    public string Detalhamento { get; set; } = string.Empty;
}
