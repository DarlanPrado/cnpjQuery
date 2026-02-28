using System.Text.Json.Serialization;

namespace Api.DTOs;

public class BillingDto
{
    [JsonPropertyName("free")]
    public bool Free { get; set; }

    [JsonPropertyName("database")]
    public bool Database { get; set; }
}
