import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '@/App'
import * as cnpjApi from '@/api/cnpj'

// Mock do módulo de API — testes não fazem chamadas reais
vi.mock('@/api/cnpj', () => ({
  consultarCnpj: vi.fn(),
}))

const mockCnpjResponse: cnpjApi.CnpjResponse = {
  cnpj: '12.345.678/0001-95',
  status: 'OK',
  nome: 'EMPRESA TESTE LTDA',
  fantasia: 'TESTE',
  situacao: 'ATIVA',
  tipo: 'MATRIZ',
  porte: 'MICRO EMPRESA',
  abertura: '01/01/2010',
  naturezaJuridica: '206-2 - Sociedade Empresária Limitada',
  capitalSocial: '10.000,00',
  logradouro: 'RUA TESTE',
  numero: '100',
  complemento: 'SALA 1',
  bairro: 'CENTRO',
  municipio: 'SAO PAULO',
  uf: 'SP',
  cep: '01310-100',
  telefone: '(11) 1234-5678',
  email: 'contato@teste.com.br',
  efr: '',
  dataSituacao: '01/01/2010',
  motivoSituacao: '',
  situacaoEspecial: '',
  dataSituacaoEspecial: '',
  ultimaAtualizacao: '2024-01-01',
  atividadePrincipal: [{ code: '62.01-5-01', text: 'Desenvolvimento de programas de computador sob encomenda' }],
  atividadesSecundarias: [],
  qsa: [{ nome: 'JOAO SILVA', qual: 'Sócio-Administrador', pais_origem: '', nome_rep_legal: '', qual_rep_legal: '' }],
}

describe('App', () => {
  const user = userEvent.setup()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renderiza o formulário de consulta', () => {
    render(<App />)
    expect(screen.getByText('Consulta de CNPJ')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('00.000.000/0000-00')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /consultar/i })).toBeInTheDocument()
  })

  it('botão está desabilitado com CNPJ incompleto', () => {
    render(<App />)
    expect(screen.getByRole('button', { name: /consultar/i })).toBeDisabled()
  })

  it('aplica a máscara ao digitar no input', async () => {
    render(<App />)
    const input = screen.getByPlaceholderText('00.000.000/0000-00')
    await user.type(input, '12345678000195')
    expect(input).toHaveValue('12.345.678/0001-95')
  })

  it('botão fica habilitado com CNPJ completo', async () => {
    render(<App />)
    const input = screen.getByPlaceholderText('00.000.000/0000-00')
    await user.type(input, '12345678000195')
    expect(screen.getByRole('button', { name: /consultar/i })).toBeEnabled()
  })

  it('chama consultarCnpj com os dígitos sem máscara ao submeter', async () => {
    vi.mocked(cnpjApi.consultarCnpj).mockResolvedValue(mockCnpjResponse)
    render(<App />)
    const input = screen.getByPlaceholderText('00.000.000/0000-00')
    await user.type(input, '12345678000195')
    await user.click(screen.getByRole('button', { name: /consultar/i }))
    expect(cnpjApi.consultarCnpj).toHaveBeenCalledWith('12345678000195')
  })

  it('exibe os dados da empresa após consulta bem-sucedida', async () => {
    vi.mocked(cnpjApi.consultarCnpj).mockResolvedValue(mockCnpjResponse)
    render(<App />)
    const input = screen.getByPlaceholderText('00.000.000/0000-00')
    await user.type(input, '12345678000195')
    await user.click(screen.getByRole('button', { name: /consultar/i }))
    await waitFor(() => {
      expect(screen.getByText('EMPRESA TESTE LTDA')).toBeInTheDocument()
      // "ATIVA" aparece no badge e na linha chave-valor — confirma ambos
      expect(screen.getAllByText('ATIVA')).toHaveLength(2)
      expect(screen.getByText('SAO PAULO / SP')).toBeInTheDocument()
    })
  })

  it('exibe mensagem de erro quando a API falha', async () => {
    vi.mocked(cnpjApi.consultarCnpj).mockRejectedValue(new Error('Erro 404: Not Found'))
    render(<App />)
    const input = screen.getByPlaceholderText('00.000.000/0000-00')
    await user.type(input, '12345678000195')
    await user.click(screen.getByRole('button', { name: /consultar/i }))
    await waitFor(() => {
      expect(screen.getByText('Erro 404: Not Found')).toBeInTheDocument()
    })
  })
})
