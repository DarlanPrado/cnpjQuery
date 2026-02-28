using System.Text.Json.Serialization;

namespace Api.DTOs;

/// <summary>Simples/Simei info as returned by the Receita Federal endpoint.</summary>
public class RfSimplesDto
{
    [JsonPropertyName("optante")]
    public bool Optante { get; set; }

    [JsonPropertyName("data_opcao")]
    public string? DataOpcao { get; set; }

    [JsonPropertyName("data_exclusao")]
    public string? DataExclusao { get; set; }

    [JsonPropertyName("ultima_atualizacao")]
    public string? UltimaAtualizacao { get; set; }
}
