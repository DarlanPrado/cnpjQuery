using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Api.DTOs;
using Api.Services;

namespace Api.Tests.Services;

public class ReceitaFederalServiceTests
{
    private static ReceitaFederalService CreateService(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        return new ReceitaFederalService(client);
    }

    private static string SerializeJson(object obj) =>
        JsonSerializer.Serialize(obj, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    // ── cenários de sucesso ──────────────────────────────────────────────────

    [Fact]
    public async Task ConsultarAsync_RespostaValida_DeserializaCorretamente()
    {
        var expected = new ReceitaFederalResponseDto
        {
            Cnpj     = "12345678000195",
            Status   = "OK",
            Nome     = "EMPRESA TESTE LTDA",
            Situacao = "ATIVA",
            Uf       = "SP",
            AtividadePrincipal = [new AtividadeDto { Code = "62.01-5-01", Text = "Desenvolvimento de software" }],
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SerializeJson(expected), Encoding.UTF8, "application/json")
        };

        var service = CreateService(response);
        var result = await service.ConsultarAsync("12345678000195");

        result.Should().NotBeNull();
        result.Cnpj.Should().Be("12345678000195");
        result.Status.Should().Be("OK");
        result.Nome.Should().Be("EMPRESA TESTE LTDA");
        result.Uf.Should().Be("SP");
        result.AtividadePrincipal.Should().HaveCount(1);
        result.AtividadePrincipal[0].Code.Should().Be("62.01-5-01");
    }

    [Fact]
    public async Task ConsultarAsync_ChamaNdpointCorreto()
    {
        var capturedRequest = default(HttpRequestMessage);

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SerializeJson(new ReceitaFederalResponseDto()), Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var service = new ReceitaFederalService(client);

        await service.ConsultarAsync("12345678000195");

        capturedRequest!.RequestUri!.ToString()
            .Should().Contain("cnpj/12345678000195/days/365");
    }

    [Fact]
    public async Task ConsultarAsync_ComSimples_DeserializaOptante()
    {
        var src = new ReceitaFederalResponseDto
        {
            Simples = new RfSimplesDto { Optante = true, DataOpcao = "2022-01-01" },
            Simei   = new RfSimplesDto { Optante = false },
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SerializeJson(src), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).ConsultarAsync("12345678000195");

        result.Simples.Should().NotBeNull();
        result.Simples!.Optante.Should().BeTrue();
        result.Simples.DataOpcao.Should().Be("2022-01-01");
        result.Simei!.Optante.Should().BeFalse();
    }

    // ── cenários de erro ─────────────────────────────────────────────────────

    [Fact]
    public async Task ConsultarAsync_Resposta404_LancaHttpRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var service = CreateService(response);

        var act = () => service.ConsultarAsync("12345678000195");
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ConsultarAsync_Resposta500_LancaHttpRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var service = CreateService(response);

        var act = () => service.ConsultarAsync("12345678000195");
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
