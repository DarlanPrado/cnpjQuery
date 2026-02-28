using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api.Controllers;
using Api.DTOs;
using Api.Services;

namespace Api.Tests.Controllers;

public class CnpjControllerTests
{
    private readonly Mock<IReceitaFederalService> _rfMock = new();
    private readonly Mock<ISimplesNacionalService> _snMock = new();

    private CnpjController CreateController() =>
        new(_rfMock.Object, _snMock.Object);

    // ── helpers ─────────────────────────────────────────────────────────────

    private ReceitaFederalResponseDto DefaultRf(string cnpj = "12345678000195") =>
        new()
        {
            Cnpj     = cnpj,
            Status   = "OK",
            Nome     = "EMPRESA TESTE LTDA",
            Situacao = "ATIVA",
        };

    private SimplesNacionalResponseDto DefaultSn() => new();

    private void SetupServices(string cnpj, ReceitaFederalResponseDto? rf = null, SimplesNacionalResponseDto? sn = null)
    {
        _rfMock.Setup(s => s.ConsultarAsync(cnpj)).ReturnsAsync(rf ?? DefaultRf(cnpj));
        _snMock.Setup(s => s.ConsultarAsync(cnpj)).ReturnsAsync(sn ?? DefaultSn());
    }

    // ── validação ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_CnpjComLetras_RetornaBadRequest()
    {
        var result = await CreateController().Get("1234567800019A");
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Get_CnpjComMenosDe14Digitos_RetornaBadRequest()
    {
        var result = await CreateController().Get("1234567");
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Get_CnpjComMaisDe14Digitos_RetornaBadRequest()
    {
        var result = await CreateController().Get("123456780001950000");
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Get_CnpjValido_Retorna200ComDados()
    {
        SetupServices("12345678000195");

        var result = await CreateController().Get("12345678000195");

        result.Result.Should().BeOfType<OkObjectResult>();
        var dto = ((OkObjectResult)result.Result!).Value.Should().BeOfType<CnpjResponseDto>().Subject;
        dto.Nome.Should().Be("EMPRESA TESTE LTDA");
        dto.Situacao.Should().Be("ATIVA");
    }

    [Fact]
    public async Task Get_CnpjComMascara_RemascaMascaraEConsulta()
    {
        // CNPJ formatado → serviços devem receber apenas dígitos
        SetupServices("12345678000195");

        var result = await CreateController().Get("12.345.678/0001-95");

        result.Result.Should().BeOfType<OkObjectResult>();
        _rfMock.Verify(s => s.ConsultarAsync("12345678000195"), Times.Once);
        _snMock.Verify(s => s.ConsultarAsync("12345678000195"), Times.Once);
    }

    [Fact]
    public async Task Get_CnpjValido_ChamaAmbosServicesEmParalelo()
    {
        SetupServices("12345678000195");

        await CreateController().Get("12345678000195");

        _rfMock.Verify(s => s.ConsultarAsync("12345678000195"), Times.Once);
        _snMock.Verify(s => s.ConsultarAsync("12345678000195"), Times.Once);
    }

    // ── MergeSimples ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_RfESnComSimples_RfTemPrioridadeESnContribuiHistorico()
    {
        var historico = new HistoricoDto();
        var rf = DefaultRf();
        rf.Simples = new RfSimplesDto
        {
            Optante      = true,
            DataOpcao    = "2020-01-01",
            DataExclusao = "2023-01-01",
            UltimaAtualizacao = "2024-01-01",
        };

        var sn = new SimplesNacionalResponseDto
        {
            Simples = new SnSimplesDto
            {
                Optante   = false,
                DataOpcao = "2019-01-01",
                Historico = historico,
            }
        };

        SetupServices("12345678000195", rf, sn);
        var result = await CreateController().Get("12345678000195");

        var dto = ((OkObjectResult)result.Result!).Value.Should().BeOfType<CnpjResponseDto>().Subject;
        dto.Simples.Should().NotBeNull();
        dto.Simples!.Optante.Should().BeTrue();            // RF tem prioridade
        dto.Simples.DataOpcao.Should().Be("2020-01-01");  // RF tem prioridade
        dto.Simples.DataExclusao.Should().Be("2023-01-01");
        dto.Simples.UltimaAtualizacao.Should().Be("2024-01-01");
        dto.Simples.Historico.Should().BeSameAs(historico); // SN contribui
    }

    [Fact]
    public async Task Get_ApenasRfComSimples_HistoricoNulo()
    {
        var rf = DefaultRf();
        rf.Simples = new RfSimplesDto { Optante = true, DataOpcao = "2020-01-01" };

        SetupServices("12345678000195", rf);
        var result = await CreateController().Get("12345678000195");

        var dto = ((OkObjectResult)result.Result!).Value.Should().BeOfType<CnpjResponseDto>().Subject;
        dto.Simples!.Optante.Should().BeTrue();
        dto.Simples.DataOpcao.Should().Be("2020-01-01");
        dto.Simples.Historico.Should().BeNull();
    }

    [Fact]
    public async Task Get_ApenasSnComSimples_OptanteEDataOpcaoDeSnSemDataExclusao()
    {
        var sn = new SimplesNacionalResponseDto
        {
            Simples = new SnSimplesDto { Optante = true, DataOpcao = "2021-06-01" }
        };

        SetupServices("12345678000195", sn: sn);
        var result = await CreateController().Get("12345678000195");

        var dto = ((OkObjectResult)result.Result!).Value.Should().BeOfType<CnpjResponseDto>().Subject;
        dto.Simples!.Optante.Should().BeTrue();
        dto.Simples.DataOpcao.Should().Be("2021-06-01");
        dto.Simples.DataExclusao.Should().BeNull();
    }

    [Fact]
    public async Task Get_SemSimplesEmAmbos_SimplesNulo()
    {
        SetupServices("12345678000195");
        var result = await CreateController().Get("12345678000195");

        var dto = ((OkObjectResult)result.Result!).Value.Should().BeOfType<CnpjResponseDto>().Subject;
        dto.Simples.Should().BeNull();
        dto.Simei.Should().BeNull();
    }
}
