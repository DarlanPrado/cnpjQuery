using System.Text.Json.Serialization;

namespace Api.DTOs;

public class HistoricoDto
{
    [JsonPropertyName("periodos_anteriores")]
    public List<PeriodoAnteriorDto> PeriodosAnteriores { get; set; } = [];
}
