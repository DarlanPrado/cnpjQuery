using System.Text.Json.Serialization;

namespace Api.DTOs;

public class ReceitaFederalResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("ultima_atualizacao")]
    public string? UltimaAtualizacao { get; set; }

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("porte")]
    public string Porte { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("fantasia")]
    public string Fantasia { get; set; } = string.Empty;

    [JsonPropertyName("abertura")]
    public string Abertura { get; set; } = string.Empty;

    [JsonPropertyName("atividade_principal")]
    public List<AtividadeDto> AtividadePrincipal { get; set; } = [];

    [JsonPropertyName("atividades_secundarias")]
    public List<AtividadeDto> AtividadesSecundarias { get; set; } = [];

    [JsonPropertyName("natureza_juridica")]
    public string NaturezaJuridica { get; set; } = string.Empty;

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; } = string.Empty;

    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("complemento")]
    public string Complemento { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("efr")]
    public string Efr { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("data_situacao")]
    public string DataSituacao { get; set; } = string.Empty;

    [JsonPropertyName("motivo_situacao")]
    public string MotivoSituacao { get; set; } = string.Empty;

    [JsonPropertyName("situacao_especial")]
    public string SituacaoEspecial { get; set; } = string.Empty;

    [JsonPropertyName("data_situacao_especial")]
    public string DataSituacaoEspecial { get; set; } = string.Empty;

    [JsonPropertyName("capital_social")]
    public string CapitalSocial { get; set; } = string.Empty;

    [JsonPropertyName("qsa")]
    public List<QsaDto> Qsa { get; set; } = [];

    [JsonPropertyName("simples")]
    public RfSimplesDto? Simples { get; set; }

    [JsonPropertyName("simei")]
    public RfSimplesDto? Simei { get; set; }

    [JsonPropertyName("billing")]
    public BillingDto? Billing { get; set; }
}
