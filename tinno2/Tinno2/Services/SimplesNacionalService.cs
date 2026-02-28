using System.Text.Json;
using Api.DTOs;

namespace Api.Services;

public class SimplesNacionalService : ISimplesNacionalService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SimplesNacionalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SimplesNacionalResponseDto> ConsultarAsync(string cnpj)
    {
        var response = await _httpClient.GetAsync($"simples/{cnpj}/days/365");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<SimplesNacionalResponseDto>(content, _jsonOptions)
               ?? throw new InvalidOperationException("Falha ao desserializar resposta do Simples Nacional.");
    }
}
