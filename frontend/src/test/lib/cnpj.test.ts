import { describe, it, expect } from 'vitest'
import { formatCnpj } from '@/lib/cnpj'

describe('formatCnpj', () => {
  it('retorna string vazia para input vazio', () => {
    expect(formatCnpj('')).toBe('')
  })

  it('formata os 2 primeiros dígitos com ponto', () => {
    expect(formatCnpj('12')).toBe('12')
    expect(formatCnpj('123')).toBe('12.3')
  })

  it('formata bloco XX.XXX', () => {
    expect(formatCnpj('12345')).toBe('12.345')
    expect(formatCnpj('123456')).toBe('12.345.6')
  })

  it('formata bloco XX.XXX.XXX', () => {
    expect(formatCnpj('12345678')).toBe('12.345.678')
  })

  it('formata bloco XX.XXX.XXX/XXXX', () => {
    expect(formatCnpj('123456780001')).toBe('12.345.678/0001')
  })

  it('formata CNPJ completo XX.XXX.XXX/XXXX-XX', () => {
    expect(formatCnpj('12345678000195')).toBe('12.345.678/0001-95')
  })

  it('remove caracteres não numéricos antes de formatar', () => {
    expect(formatCnpj('12.345.678/0001-95')).toBe('12.345.678/0001-95')
    expect(formatCnpj('12-345-678/0001.95')).toBe('12.345.678/0001-95')
  })

  it('limita a 14 dígitos', () => {
    expect(formatCnpj('123456780001956789')).toBe('12.345.678/0001-95')
  })
})
