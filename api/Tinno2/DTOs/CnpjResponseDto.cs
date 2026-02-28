namespace Api.DTOs;

public class CnpjResponseDto
{
    public string Cnpj { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? UltimaAtualizacao { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Porte { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Fantasia { get; set; } = string.Empty;
    public string Abertura { get; set; } = string.Empty;
    public List<AtividadeDto> AtividadePrincipal { get; set; } = [];
    public List<AtividadeDto> AtividadesSecundarias { get; set; } = [];
    public string NaturezaJuridica { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Efr { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string DataSituacao { get; set; } = string.Empty;
    public string MotivoSituacao { get; set; } = string.Empty;
    public string SituacaoEspecial { get; set; } = string.Empty;
    public string DataSituacaoEspecial { get; set; } = string.Empty;
    public string CapitalSocial { get; set; } = string.Empty;
    public List<QsaDto> Qsa { get; set; } = [];
    public SimplesUnificadoDto? Simples { get; set; }
    public SimplesUnificadoDto? Simei { get; set; }
}
