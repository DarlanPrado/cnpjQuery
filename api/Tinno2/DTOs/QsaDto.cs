using System.Text.Json.Serialization;

namespace Api.DTOs;

public class QsaDto
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("qual")]
    public string Qual { get; set; } = string.Empty;

    [JsonPropertyName("pais_origem")]
    public string PaisOrigem { get; set; } = string.Empty;

    [JsonPropertyName("nome_rep_legal")]
    public string NomeRepLegal { get; set; } = string.Empty;

    [JsonPropertyName("qual_rep_legal")]
    public string QualRepLegal { get; set; } = string.Empty;
}
