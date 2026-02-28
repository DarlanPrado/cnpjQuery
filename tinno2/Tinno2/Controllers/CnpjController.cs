using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/cnpj")]
public class CnpjController : ControllerBase
{
    private readonly IReceitaFederalService _receitaFederalService;
    private readonly ISimplesNacionalService _simplesNacionalService;

    public CnpjController(
        IReceitaFederalService receitaFederalService,
        ISimplesNacionalService simplesNacionalService)
    {
        _receitaFederalService = receitaFederalService;
        _simplesNacionalService = simplesNacionalService;
    }

    [HttpGet("{cnpj}")]
    public async Task<ActionResult<CnpjResponseDto>> Get(string cnpj)
    {
        var cnpjDigits = Regex.Replace(cnpj, @"[.\-/]", "");

        if (!Regex.IsMatch(cnpjDigits, @"^\d+$"))
            return BadRequest("CNPJ inválido: deve conter apenas números.");

        if (cnpjDigits.Length != 14)
            return BadRequest("CNPJ inválido: deve conter 14 dígitos.");

        var (rf, sn) = await FetchBothAsync(cnpjDigits);

        var response = new CnpjResponseDto
        {
            Cnpj             = cnpjDigits,
            Status           = rf.Status,
            UltimaAtualizacao = rf.UltimaAtualizacao,
            Tipo             = rf.Tipo,
            Porte            = rf.Porte,
            Nome             = rf.Nome,
            Fantasia         = rf.Fantasia,
            Abertura         = rf.Abertura,
            AtividadePrincipal    = rf.AtividadePrincipal,
            AtividadesSecundarias = rf.AtividadesSecundarias,
            NaturezaJuridica = rf.NaturezaJuridica,
            Logradouro       = rf.Logradouro,
            Numero           = rf.Numero,
            Complemento      = rf.Complemento,
            Cep              = rf.Cep,
            Bairro           = rf.Bairro,
            Municipio        = rf.Municipio,
            Uf               = rf.Uf,
            Email            = rf.Email,
            Telefone         = rf.Telefone,
            Efr              = rf.Efr,
            Situacao         = rf.Situacao,
            DataSituacao     = rf.DataSituacao,
            MotivoSituacao   = rf.MotivoSituacao,
            SituacaoEspecial = rf.SituacaoEspecial,
            DataSituacaoEspecial = rf.DataSituacaoEspecial,
            CapitalSocial    = rf.CapitalSocial,
            Qsa              = rf.Qsa,
            Simples = MergeSimples(rf.Simples, sn.Simples),
            Simei   = MergeSimples(rf.Simei,   sn.Simei),
        };

        return Ok(response);
    }

    private async Task<(ReceitaFederalResponseDto rf, SimplesNacionalResponseDto sn)> FetchBothAsync(string cnpj)
    {
        var rfTask = _receitaFederalService.ConsultarAsync(cnpj);
        var snTask = _simplesNacionalService.ConsultarAsync(cnpj);
        await Task.WhenAll(rfTask, snTask);
        return (await rfTask, await snTask);
    }

    private static SimplesUnificadoDto? MergeSimples(RfSimplesDto? rf, SnSimplesDto? sn)
    {
        if (rf is null && sn is null) return null;

        return new SimplesUnificadoDto
        {
            Optante          = rf?.Optante ?? sn?.Optante ?? false,
            DataOpcao        = rf?.DataOpcao ?? sn?.DataOpcao,
            DataExclusao     = rf?.DataExclusao,
            UltimaAtualizacao = rf?.UltimaAtualizacao,
            Historico        = sn?.Historico,
        };
    }
}
