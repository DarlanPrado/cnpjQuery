using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Api.DTOs;
using Api.Services;

namespace Api.Tests.Services;

public class SimplesNacionalServiceTests
{
    private static SimplesNacionalService CreateService(HttpResponseMessage response)
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
        return new SimplesNacionalService(client);
    }

    private static string SerializeJson(object obj) =>
        JsonSerializer.Serialize(obj, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    // ── cenários de sucesso ──────────────────────────────────────────────────

    [Fact]
    public async Task ConsultarAsync_RespostaValida_DeserializaCorretamente()
    {
        var expected = new SimplesNacionalResponseDto
        {
            Cnpj    = "12345678000195",
            Simples = new SnSimplesDto { Optante = true, DataOpcao = "2020-01-01" },
            Simei   = new SnSimplesDto { Optante = false },
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SerializeJson(expected), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).ConsultarAsync("12345678000195");

        result.Should().NotBeNull();
        result.Cnpj.Should().Be("12345678000195");
        result.Simples.Should().NotBeNull();
        result.Simples!.Optante.Should().BeTrue();
        result.Simples.DataOpcao.Should().Be("2020-01-01");
        result.Simei!.Optante.Should().BeFalse();
    }

    [Fact]
    public async Task ConsultarAsync_ChamaEndpointCorreto()
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
                Content = new StringContent(SerializeJson(new SimplesNacionalResponseDto()), Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var service = new SimplesNacionalService(client);

        await service.ConsultarAsync("12345678000195");

        capturedRequest!.RequestUri!.ToString()
            .Should().Contain("simples/12345678000195/days/365");
    }

    [Fact]
    public async Task ConsultarAsync_ComHistorico_DeserializaPeriodosAnteriores()
    {
        var expected = new SimplesNacionalResponseDto
        {
            Simples = new SnSimplesDto
            {
                Optante = true,
                Historico = new HistoricoDto
                {
                    PeriodosAnteriores =
                    [
                        new PeriodoAnteriorDto { Inicio = "2018-01-01", Fim = "2019-12-31", Detalhamento = "Optante" }
                    ]
                }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SerializeJson(expected), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).ConsultarAsync("12345678000195");

        result.Simples!.Historico.Should().NotBeNull();
        result.Simples.Historico!.PeriodosAnteriores.Should().HaveCount(1);
        result.Simples.Historico.PeriodosAnteriores[0].Inicio.Should().Be("2018-01-01");
    }

    [Fact]
    public async Task ConsultarAsync_SemSimplesNemSimei_PropriedadesNulas()
    {
        var expected = new SimplesNacionalResponseDto { Cnpj = "12345678000195" };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SerializeJson(expected), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).ConsultarAsync("12345678000195");

        result.Simples.Should().BeNull();
        result.Simei.Should().BeNull();
    }

    // ── cenários de erro ─────────────────────────────────────────────────────

    [Fact]
    public async Task ConsultarAsync_Resposta404_LancaHttpRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var act = () => CreateService(response).ConsultarAsync("12345678000195");
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ConsultarAsync_Resposta500_LancaHttpRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var act = () => CreateService(response).ConsultarAsync("12345678000195");
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
