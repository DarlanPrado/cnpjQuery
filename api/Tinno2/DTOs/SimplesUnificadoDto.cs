namespace Api.DTOs;

/// <summary>Merged Simples/Simei block: core fields from Receita Federal + historico from Simples Nacional.</summary>
public class SimplesUnificadoDto
{
    public bool Optante { get; set; }
    public string? DataOpcao { get; set; }
    public string? DataExclusao { get; set; }
    public string? UltimaAtualizacao { get; set; }
    public HistoricoDto? Historico { get; set; }
}
