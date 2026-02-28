using System.Text.Json.Serialization;

namespace Api.DTOs;

public class AtividadeDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
