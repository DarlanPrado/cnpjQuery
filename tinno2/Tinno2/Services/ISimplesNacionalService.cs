using Api.DTOs;

namespace Api.Services;

public interface ISimplesNacionalService
{
    Task<SimplesNacionalResponseDto> ConsultarAsync(string cnpj);
}
