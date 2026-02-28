import { apiClient } from './client'
import type { components } from './schema.d.ts'

export type CnpjResponse = components['schemas']['CnpjResponseDto']
export type AtividadeDto = components['schemas']['AtividadeDto']
export type QsaDto = components['schemas']['QsaDto']
export type SimplesUnificadoDto = components['schemas']['SimplesUnificadoDto']

export async function consultarCnpj(cnpj: string): Promise<CnpjResponse> {
  const { data, error, response } = await apiClient.GET('/api/cnpj/{cnpj}', {
    params: { path: { cnpj } },
  })

  if (error || !data) {
    throw new Error(`Erro ${response.status}: ${response.statusText}`)
  }

  return data
}
