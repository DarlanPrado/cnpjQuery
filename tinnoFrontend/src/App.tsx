import { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { consultarCnpj, type CnpjResponse } from '@/api/cnpj'
import { formatCnpj } from '@/lib/cnpj'

function Row({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null
  return (
    <div className="grid grid-cols-[180px_1fr] gap-2 py-1.5 text-sm">
      <span className="text-muted-foreground font-medium">{label}</span>
      <span className="text-foreground break-words">{value}</span>
    </div>
  )
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground mt-4 mb-1">
        {title}
      </h2>
      <Separator />
      {children}
    </div>
  )
}

// ---------- componente principal ----------
function App() {
  const [cnpj, setCnpj] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [data, setData] = useState<CnpjResponse | null>(null)

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    setCnpj(formatCnpj(e.target.value))
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const digits = cnpj.replace(/\D/g, '')
    setLoading(true)
    setError(null)
    setData(null)
    try {
      const json = await consultarCnpj(digits)
      setData(json)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao consultar CNPJ')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-background px-4 py-10">
      <div className="mx-auto w-full max-w-2xl flex flex-col gap-6">

        {/* formulário */}
        <form
          onSubmit={handleSubmit}
          className="flex flex-col gap-4 p-8 rounded-xl border border-border shadow-sm bg-card"
        >
          <h1 className="text-2xl font-bold tracking-tight">Consulta de CNPJ</h1>
          <div className="flex gap-2">
            <Input
              id="cnpj"
              placeholder="00.000.000/0000-00"
              value={cnpj}
              onChange={handleChange}
              inputMode="numeric"
              maxLength={18}
              className="flex-1"
            />
            <Button type="submit" disabled={cnpj.length < 18 || loading}>
              {loading ? 'Consultando...' : 'Consultar'}
            </Button>
          </div>
          {error && (
            <p className="text-sm text-destructive">{error}</p>
          )}
        </form>

        {/* resultado */}
        {data && (
          <div className="p-8 rounded-xl border border-border shadow-sm bg-card">

            {/* cabeçalho */}
            <div className="flex items-start justify-between gap-4 flex-wrap">
              <div>
                <p className="text-xs text-muted-foreground">{data.cnpj}</p>
                <h2 className="text-xl font-bold">{data.nome}</h2>
                {data.fantasia && (
                  <p className="text-sm text-muted-foreground">{data.fantasia}</p>
                )}
              </div>
              <Badge variant={data.situacao === 'ATIVA' ? 'default' : 'destructive'}>
                {data.situacao}
              </Badge>
            </div>

            {/* dados gerais */}
            <Section title="Dados Gerais">
              <Row label="Tipo" value={data.tipo} />
              <Row label="Porte" value={data.porte} />
              <Row label="Abertura" value={data.abertura} />
              <Row label="Natureza Jurídica" value={data.naturezaJuridica} />
              <Row label="Capital Social" value={data.capitalSocial} />
              <Row label="Situação" value={data.situacao} />
              <Row label="Data Situação" value={data.dataSituacao} />
              <Row label="Motivo" value={data.motivoSituacao} />
              <Row label="Situação Especial" value={data.situacaoEspecial} />
              <Row label="Última Atualização" value={data.ultimaAtualizacao} />
            </Section>

            {/* endereço */}
            <Section title="Endereço">
              <Row label="Logradouro" value={`${data.logradouro}${data.numero ? ', ' + data.numero : ''}${data.complemento ? ' - ' + data.complemento : ''}`} />
              <Row label="Bairro" value={data.bairro} />
              <Row label="Município / UF" value={`${data.municipio} / ${data.uf}`} />
              <Row label="CEP" value={data.cep} />
            </Section>

            {/* contato */}
            <Section title="Contato">
              <Row label="Telefone" value={data.telefone} />
              <Row label="E-mail" value={data.email} />
            </Section>

            {/* atividade principal */}
            {data.atividadePrincipal?.length > 0 && (
              <Section title="Atividade Principal">
                {data.atividadePrincipal.map((a, i) => (
                  <div key={i} className="grid grid-cols-[180px_1fr] gap-2 py-1.5 text-sm">
                    <span className="text-muted-foreground font-medium">{a.code}</span>
                    <span>{a.text}</span>
                  </div>
                ))}
              </Section>
            )}

            {/* atividades secundárias */}
            {data.atividadesSecundarias?.length > 0 && (
              <Section title="Atividades Secundárias">
                {data.atividadesSecundarias.map((a, i) => (
                  <div key={i} className="grid grid-cols-[180px_1fr] gap-2 py-1.5 text-sm">
                    <span className="text-muted-foreground font-medium">{a.code}</span>
                    <span>{a.text}</span>
                  </div>
                ))}
              </Section>
            )}

            {/* qsa */}
            {data.qsa?.length > 0 && (
              <Section title="Quadro Societário (QSA)">
                {data.qsa.map((q, i) => (
                  <div key={i} className="py-1.5 text-sm">
                    <span className="font-medium">{q.nome}</span>
                    <span className="text-muted-foreground"> — {q.qual}</span>
                    {q.nome_rep_legal && (
                      <span className="text-muted-foreground"> | Rep.: {q.nome_rep_legal} ({q.qual_rep_legal})</span>
                    )}
                  </div>
                ))}
              </Section>
            )}

            {/* simples / simei */}
            {(data.simples || data.simei) && (
              <Section title="Simples Nacional / SIMEI">
                {data.simples && (
                  <>
                    <Row label="Simples Nacional" value={data.simples.optante ? 'Optante' : 'Não optante'} />
                    <Row label="Data Opção SN" value={data.simples.dataOpcao} />
                    <Row label="Data Exclusão SN" value={data.simples.dataExclusao} />
                  </>
                )}
                {data.simei && (
                  <>
                    <Row label="SIMEI" value={data.simei.optante ? 'Optante' : 'Não optante'} />
                    <Row label="Data Opção SIMEI" value={data.simei.dataOpcao} />
                    <Row label="Data Exclusão SIMEI" value={data.simei.dataExclusao} />
                  </>
                )}
              </Section>
            )}

          </div>
        )}
      </div>
    </div>
  )
}

export default App
