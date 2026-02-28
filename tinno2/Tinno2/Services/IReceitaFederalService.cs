using Api.DTOs;

namespace Api.Services;

public interface IReceitaFederalService
{
    Task<ReceitaFederalResponseDto> ConsultarAsync(string cnpj);
}
