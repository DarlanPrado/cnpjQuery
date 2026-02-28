using System.Text.Json.Serialization;

namespace Api.DTOs;

/// <summary>Simples/Simei info as returned by the Simples Nacional endpoint.</summary>
public class SnSimplesDto
{
    [JsonPropertyName("optante")]
    public bool Optante { get; set; }

    [JsonPropertyName("data_opcao")]
    public string? DataOpcao { get; set; }

    [JsonPropertyName("historico")]
    public HistoricoDto? Historico { get; set; }
}
