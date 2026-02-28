using System.Text.Json;
using Api.DTOs;

namespace Api.Services;

public class ReceitaFederalService : IReceitaFederalService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ReceitaFederalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReceitaFederalResponseDto> ConsultarAsync(string cnpj)
    {
        var response = await _httpClient.GetAsync($"cnpj/{cnpj}/days/365");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<ReceitaFederalResponseDto>(content, _jsonOptions)
               ?? throw new InvalidOperationException("Falha ao desserializar resposta da Receita Federal.");
    }
}
