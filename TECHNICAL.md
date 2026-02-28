# TECHNICAL.md — Documentação Técnica / Technical Documentation

> Documentação aprofundada da arquitetura, tecnologias, metodologias e decisões de implementação do projeto CNPJ Query.  
> In-depth documentation of the architecture, technologies, methodologies, and implementation decisions of the CNPJ Query project.

---

## Índice / Table of Contents

1. [Stack Overview](#1-stack-overview)
2. [Arquitetura / Architecture](#2-arquitetura--architecture)
3. [Frontend](#3-frontend)
4. [Backend](#4-backend)
5. [Testes / Testing](#5-testes--testing)
6. [Docker](#6-docker)
7. [Decisões técnicas / Technical decisions](#7-decisões-técnicas--technical-decisions)

---

## 1. Stack Overview

| Camada / Layer | Tecnologia / Technology | Versão / Version | Propósito / Purpose |
|---|---|---|---|
| Frontend framework | React | 19 | UI reativa / Reactive UI |
| Build tool | Vite | 7 | Dev server, HMR, bundling |
| Linguagem / Language | TypeScript | 5.9 | Tipagem estática / Static typing |
| Estilo / Styling | Tailwind CSS | 4 | Utility-first CSS |
| Componentes / Components | shadcn/ui (radix-nova) | — | Primitivos acessíveis / Accessible primitives |
| HTTP client | openapi-fetch | 0.17.0 | Client tipado por schema / Schema-typed client |
| Schema generator | openapi-typescript | 7.13.0 | Gera tipos TS do OpenAPI / Generates TS types from OpenAPI |
| Backend framework | ASP.NET Core | 10.0 | HTTP API |
| OpenAPI docs | Microsoft.AspNetCore.OpenApi | 10.0 | Schema nativo / Native schema |
| API UI | Scalar | 2.12 | Documentação interativa / Interactive docs |
| Package manager (FE) | pnpm | 9.x | Gerenciamento de pacotes / Package management |
| Testes frontend / FE tests | Vitest + Testing Library | 4.0 / 16.0 | Unit + component tests |
| Testes backend / BE tests | xUnit + Moq + FluentAssertions | 2.9 / 4.20 / 8.8 | Unit tests |

---

## 2. Arquitetura / Architecture

### Fluxo de dados / Data flow

```
┌─────────────────────────────────────────────────────┐
│  Browser — React App (localhost:5173)               │
│  ┌───────────────────────────────────────────────┐  │
│  │ 1. Usuário digita CNPJ (máscara aplicada)    │  │
│  │ 2. formatCnpj() → "XX.XXX.XXX/XXXX-XX"      │  │
│  │ 3. consultarCnpj(digits) via openapi-fetch   │  │
│  └───────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────┘
                       │ GET /api/cnpj/{cnpj}
                       │ HTTPS · localhost:7040
                       ▼
┌─────────────────────────────────────────────────────┐
│  ASP.NET Core 10 API                                │
│  ┌───────────────────────────────────────────────┐  │
│  │ CnpjController                               │  │
│  │  ├── Valida: só dígitos, exatamente 14       │  │
│  │  ├── Task.WhenAll([RF.ConsultarAsync,        │  │
│  │  │                  SN.ConsultarAsync])       │  │
│  │  └── MergeSimples(rf, sn) → CnpjResponseDto │  │
│  └───────────────────────────────────────────────┘  │
└──────┬──────────────────────────────┬───────────────┘
       │ cnpj/{cnpj}/days/365         │ simples/{cnpj}/days/365
       ▼                              ▼
┌──────────────┐              ┌──────────────┐
│ ReceitaWS    │              │ ReceitaWS    │
│ CNPJ API     │              │ Simples API  │
└──────────────┘              └──────────────┘
```

### Merge de respostas / Response merging

O método `MergeSimples` resolve conflitos entre os dados de Simples/SIMEI retornados pelos dois endpoints:

- Campos escalares (optante, datas): resposta da Receita Federal tem **prioridade**
- Campo `Historico`: vem **exclusivamente** da Simples Nacional
- Ambas as fontes são `null`: resultado é `null`

```
MergeSimples(rf.Simples, sn.Simples) → SimplesUnificadoDto
MergeSimples(rf.Simei,   sn.Simei)   → SimplesUnificadoDto
```

---

## 3. Frontend

### 3.1 Client HTTP tipado / Type-safe HTTP client

O projeto usa `openapi-typescript` para gerar um arquivo de tipos TypeScript diretamente do schema OpenAPI exposto pelo backend:

```bash
# Requer o backend rodando em http://localhost:5163
pnpm generate:api
# → src/api/schema.d.ts  (gerado automaticamente, não editar)
```

O arquivo gerado exporta a interface `paths` com todas as rotas e tipos request/response. O `openapi-fetch` usa essa interface para prover autocompletar e checagem de tipos no momento da compilação:

```ts
// src/api/client.ts
import createClient from 'openapi-fetch'
import type { paths } from './schema.d.ts'

export const apiClient = createClient<paths>({ baseUrl: 'https://localhost:7040' })
```

```ts
// src/api/cnpj.ts
export async function consultarCnpj(cnpj: string): Promise<CnpjResponse> {
  const { data, error, response } = await apiClient.GET('/api/cnpj/{cnpj}', {
    params: { path: { cnpj } },
  })
  if (error || !data) throw new Error(`Erro ${response.status}: ${response.statusText}`)
  return data
}
```

Qualquer alteração no contrato do backend (novo campo, tipo diferente, rota renomeada) é capturado em tempo de compilação após rodar `pnpm generate:api`.

### 3.2 Máscara de CNPJ / CNPJ mask

A função `formatCnpj` em `src/lib/cnpj.ts` é extraída do componente para ser testável de forma isolada:

```ts
export function formatCnpj(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 14)
  // Aplica progressivamente: XX.XXX.XXX/XXXX-XX
}
```

### 3.3 Componentes shadcn/ui

Componentes utilizados: `Input`, `Button`, `Badge`, `Card` (`CardHeader`, `CardContent`), `Separator`. Todos instalados via CLI do shadcn (estilo **radix-nova**) e customizáveis via CSS variables do Tailwind.

### 3.4 Estrutura de pastas / Folder structure

```
src/
├── api/
│   ├── client.ts        # Singleton openapi-fetch
│   ├── cnpj.ts          # Helper consultarCnpj() + exports de tipos
│   └── schema.d.ts      # AUTO-GERADO — não editar manualmente
├── components/ui/       # Componentes shadcn/ui
├── lib/
│   ├── cnpj.ts          # formatCnpj()
│   └── utils.ts         # cn() (clsx + tailwind-merge)
├── test/
│   ├── setup.ts         # @testing-library/jest-dom global setup
│   ├── App.test.tsx     # Testes do componente App
│   └── lib/
│       └── cnpj.test.ts # Testes unitários de formatCnpj
└── App.tsx              # Componente principal
```

---

## 4. Backend

### 4.1 Organização / Organization

```
Tinno2/
├── Controllers/
│   └── CnpjController.cs        # GET /api/cnpj/{cnpj}
├── DTOs/                        # 12 Data Transfer Objects
│   ├── CnpjResponseDto.cs       # Contrato da resposta final
│   ├── ReceitaFederalResponseDto.cs  # Mapeamento do endpoint RF
│   ├── SimplesNacionalResponseDto.cs # Mapeamento do endpoint SN
│   ├── SimplesUnificadoDto.cs   # Simples/SIMEI unificado
│   ├── QsaDto.cs                # Sócio (nome, qual, rep. legal)
│   ├── AtividadeDto.cs          # Atividade (código + texto)
│   ├── HistoricoDto.cs          # Histórico Simples Nacional
│   ├── PeriodoAnteriorDto.cs    # Período do histórico
│   └── ...                      # RfSimplesDto, SnSimplesDto, BillingDto, CnpjRequestDto
└── Services/
    ├── IReceitaFederalService.cs
    ├── ReceitaFederalService.cs
    ├── ISimplesNacionalService.cs
    └── SimplesNacionalService.cs
```

### 4.2 Injeção de dependência / Dependency injection

Os serviços HTTP são registrados com `AddHttpClient<TInterface, TImpl>`, o que permite ao ASP.NET Core gerenciar o ciclo de vida do `HttpClient` (pooling de conexões, evita socket exhaustion):

```csharp
// Program.cs
builder.Services.AddHttpClient<IReceitaFederalService, ReceitaFederalService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ReceitaWS:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["ReceitaWS:Token"]);
});
```

### 4.3 OpenAPI e Scalar / OpenAPI and Scalar

O contrato OpenAPI é gerado nativamente pelo ASP.NET Core 10 (sem Swashbuckle). Em modo Development, dois endpoints são expostos:

| Rota / Route | Propósito / Purpose |
|---|---|
| `/openapi/v1.json` | Schema JSON (usado pelo `pnpm generate:api`) |
| `/scalar` | Interface interativa Scalar UI |

### 4.4 CORS

Configurado para aceitar requisições das origens do Vite:

```csharp
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins(
        "http://localhost:5173", "https://localhost:5173",
        "http://localhost:4173", "https://localhost:4173")
    .AllowAnyHeader()
    .AllowAnyMethod()));
```

### 4.5 Configuração e segredos / Configuration and secrets

| Arquivo / File | Conteúdo / Content | Commitado / Committed |
|---|---|---|
| `appsettings.json` | `BaseUrl` + `Token: "YOUR_RECEITAWS_TOKEN_HERE"` | ✅ Sim (placeholder) |
| `appsettings.Development.json` | Token real / Real token | ❌ Não (`.gitignore`) |

O token real deve estar em `appsettings.Development.json` (ambiente `Development` sobrescreve o valor base). Em produção, usar variáveis de ambiente ou User Secrets do .NET.

---

## 5. Testes / Testing

### 5.1 Frontend — Vitest 4 + Testing Library 16

**Ambiente:** `jsdom` (simulação de DOM em Node.js)  
**Setup global:** `src/test/setup.ts` importa `@testing-library/jest-dom` (matchers como `toBeInTheDocument`)

#### Testes unitários — `src/test/lib/cnpj.test.ts` (8 testes)

Testam a função `formatCnpj` isoladamente:

| Cenário / Scenario | |
|---|---|
| String vazia | Retorna `""` |
| 2 dígitos | `"12"` |
| 5 dígitos | `"12.345"` |
| 8 dígitos | `"12.345.678"` |
| 12 dígitos | `"12.345.678/9012"` |
| 14 dígitos (completo) | `"12.345.678/9012-34"` |
| Entrada já com máscara | Re-aplica corretamente |
| Mais de 14 dígitos | Trunca em 14 |

#### Testes de componente — `src/test/App.test.tsx` (7 testes)

Usam `vi.mock('@/api/cnpj')` para isolar o componente da rede:

| Cenário / Scenario | |
|---|---|
| Renderização inicial | Formulário presente, dados ausentes |
| CNPJ incompleto | Botão desabilitado |
| Digitação | Máscara aplicada progressivamente |
| CNPJ completo | Botão habilitado |
| Submit | `consultarCnpj` chamado com 14 dígitos sem máscara |
| Sucesso | Dados da empresa exibidos (`Badge` ATIVA + campos) |
| Erro da API | Mensagem de erro exibida |

### 5.2 Backend — xUnit + Moq + FluentAssertions

**Isolamento de HTTP:** O `HttpClient` é instanciado diretamente nos testes com um `HttpMessageHandler` mockado via `Moq.Protected()`, sem necessidade de servidor real:

```csharp
var handlerMock = new Mock<HttpMessageHandler>();
handlerMock.Protected()
    .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
    });

var client = new HttpClient(handlerMock.Object)
    { BaseAddress = new Uri("https://receitaws.com.br/v1/") };
var service = new ReceitaFederalService(client);
```

**FluentAssertions** é usado para asserções legíveis:

```csharp
result.Should().NotBeNull();
result.Nome.Should().Be("EMPRESA EXEMPLO LTDA");
result.Situacao.Should().Be("ATIVA");
```

#### CnpjControllerTests — 10 testes

| Cenário / Scenario | HTTP status |
|---|---|
| CNPJ com letras | 400 |
| Menos de 14 dígitos | 400 |
| Mais de 14 dígitos | 400 |
| CNPJ válido (14 dígitos) | 200 |
| CNPJ com máscara (`XX.XXX.XXX/XXXX-XX`) | 200 (máscara removida) |
| Ambos os serviços chamados em paralelo | Verifica `Task.WhenAll` |
| `MergeSimples` — RF e SN contribuem | Campos de ambas as fontes |
| `MergeSimples` — só RF | Historico null |
| `MergeSimples` — só SN | Campos SN usados |
| `MergeSimples` — nenhuma fonte | null |

#### ReceitaFederalServiceTests — 5 testes

| Cenário / Scenario | |
|---|---|
| Desserialização correta do JSON | Todos os campos mapeados |
| Endpoint correto | `cnpj/{cnpj}/days/365` |
| Desserialização de Simples/SIMEI | Campos `optante`, datas |
| HTTP 404 | Lança `HttpRequestException` |
| HTTP 500 | Lança `HttpRequestException` |

#### SimplesNacionalServiceTests — 6 testes

| Cenário / Scenario | |
|---|---|
| Desserialização correta do JSON | Todos os campos mapeados |
| Endpoint correto | `simples/{cnpj}/days/365` |
| Histórico com períodos anteriores | Lista `PeriodosAnteriores` corretamente |
| Simples e SIMEI nulos | Retorna sem erros |
| HTTP 404 | Lança `HttpRequestException` |
| HTTP 500 | Lança `HttpRequestException` |

---

## 6. Docker

O backend inclui um `Dockerfile` multi-stage para build e deploy em Linux:

```dockerfile
# Estágio 1: base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
EXPOSE 8080 8081

# Estágio 2: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# restore → build (Release)

# Estágio 3: publish
# dotnet publish /p:UseAppHost=false

# Estágio 4: final
FROM base AS final
COPY --from=publish ...
ENTRYPOINT ["dotnet", "Tinno2.dll"]
```

Em produção, configurar o token via variável de ambiente:

```bash
docker run -e ReceitaWS__Token=SEU_TOKEN cnpjquery
```

---

## 7. Decisões técnicas / Technical decisions

### Por que `openapi-fetch` em vez de `fetch` manual?

Usar `fetch` manual exigiria definir e manter interfaces TypeScript manualmente em paralelo ao backend. Com `openapi-typescript` + `openapi-fetch`, o contrato é a **única fonte de verdade (single source of truth)**: qualquer mudança no backend quebra a compilação do frontend, forçando atualização explícita. O custo de setup é baixo (`pnpm generate:api`) e o ganho em segurança de tipos é significativo.

### Por que chamadas paralelas (`Task.WhenAll`)?

Os dois endpoints da ReceitaWS (`cnpj/` e `simples/`) são independentes. Chamá-los em sequência dobraria a latência da resposta. `Task.WhenAll` executa ambos concorrentemente, reduzindo o tempo de resposta ao valor da chamada mais lenta (em vez da soma das duas).

### Por que RF tem prioridade no `MergeSimples`?

O endpoint de dados gerais da Receita Federal retorna informações de Simples/SIMEI como parte do registro cadastral completo. Ele é considerado mais autoritativo para dados escalares. O endpoint Simples Nacional contribui com o histórico (`Historico`), que não está disponível no endpoint RF.

### Por que `HttpClient` com `HttpMessageHandler` mockado e não `WebApplicationFactory`?

Os testes de serviço visam testar **apenas a camada de serialização e chamada HTTP** de cada serviço isoladamente, sem precisar subir toda a pipeline do ASP.NET Core. Mockar o `HttpMessageHandler` é mais rápido, mais direto e não tem dependências externas. `Microsoft.AspNetCore.Mvc.Testing` (que inclui `WebApplicationFactory`) está disponível no projeto mas é mais adequado para testes de integração end-to-end.

### Por que `appsettings.Development.json` para o token e não User Secrets?

Para simplificar o onboarding local. `appsettings.Development.json` está no `.gitignore`, então é seguro para desenvolvimento. Em produção ou CI, a recomendação é usar variáveis de ambiente (`ReceitaWS__Token=...`) ou o mecanismo de secrets do ambiente de deploy (ex.: GitHub Actions Secrets, Azure Key Vault).
