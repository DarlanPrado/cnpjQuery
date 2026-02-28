# CNPJ Query

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript)
![Vitest](https://img.shields.io/badge/Vitest-4.0-6E9F18?logo=vitest)
![xUnit](https://img.shields.io/badge/xUnit-2.9-blueviolet)

---

## O que é / What is it

**PT:** Aplicação web para consulta de dados de CNPJ. Ao inserir um CNPJ, o sistema consulta a API da ReceitaWS em paralelo nos endpoints de dados gerais e Simples Nacional, unifica as respostas e exibe as informações completas da empresa — dados cadastrais, endereço, quadro societário (QSA) e situação no Simples Nacional / SIMEI.

**EN:** Web application for querying CNPJ (Brazilian company registration) data. Given a CNPJ, the system concurrently fetches data from the ReceitaWS API (general data + Simples Nacional endpoints), merges both responses, and displays full company information — registration details, address, ownership structure (QSA), and Simples Nacional / SIMEI status.

---

## Resumo técnico / Tech snapshot

**PT:** O frontend é construído em React 19 com client HTTP tipado gerado a partir do schema OpenAPI do backend. O backend é uma API ASP.NET Core 10 que orquestra chamadas paralelas a uma API externa e expõe um contrato OpenAPI nativo. Ambas as camadas possuem cobertura de testes automatizados.

**EN:** The frontend is built in React 19 with a type-safe HTTP client generated from the backend's OpenAPI schema. The backend is an ASP.NET Core 10 API that orchestrates parallel calls to an external API and exposes a native OpenAPI contract. Both layers have automated test coverage.

| Camada / Layer | Stack |
|---|---|
| Frontend | React 19, Vite 7, TypeScript 5.9, Tailwind CSS 4, shadcn/ui |
| API Client | `openapi-fetch` + schema gerado por `openapi-typescript` |
| Backend | ASP.NET Core 10, OpenAPI nativo, Scalar UI |
| Testes frontend / Frontend tests | Vitest 4, Testing Library 16, jsdom — **15 testes** |
| Testes backend / Backend tests | xUnit, Moq, FluentAssertions — **21 testes** |

> Para documentação técnica aprofundada, veja [TECHNICAL.md](TECHNICAL.md).  
> For in-depth technical documentation, see [TECHNICAL.md](TECHNICAL.md).

---

## Arquitetura / Architecture

```
Browser (React · porta/port 5173)
        │
        │  GET /api/cnpj/{cnpj}  (HTTPS · porta/port 7040)
        ▼
ASP.NET Core 10 API
        │
        │  Task.WhenAll
        ├──► GET https://receitaws.com.br/v1/cnpj/{cnpj}/days/365
        └──► GET https://receitaws.com.br/v1/simples/{cnpj}/days/365
        │
        │  merge → CnpjResponseDto
        ▼
     JSON response
```

---

## Pré-requisitos / Prerequisites

| Ferramenta / Tool | Versão mínima / Min version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [Node.js](https://nodejs.org) | 22.x |
| [pnpm](https://pnpm.io) | 9.x |
| Token da API ReceitaWS / ReceitaWS API token | — |

> Obtenha um token em [receitaws.com.br](https://receitaws.com.br).  
> Get a token at [receitaws.com.br](https://receitaws.com.br).

---

## Como rodar localmente / Running locally

### 1. Backend

```bash
# Clone o repositório / Clone the repository
git clone https://github.com/DarlanPrado/cnpjQuery.git
cd cnpjQuery
```

Configure seu token da ReceitaWS em / Set your ReceitaWS token in  
`tinno2/Tinno2/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ReceitaWS": {
    "Token": "SEU_TOKEN_AQUI"
  }
}
```

```bash
cd tinno2/Tinno2
dotnet run
# API disponível em / API available at: https://localhost:7040
# Scalar UI (docs): https://localhost:7040/scalar
```

### 2. Frontend

```bash
# Em outro terminal / In another terminal
cd tinnoFrontend

# Instalar dependências / Install dependencies
pnpm install

# Gerar client tipado a partir do schema OpenAPI (backend deve estar rodando)
# Generate typed client from OpenAPI schema (backend must be running)
pnpm generate:api

# Iniciar servidor de desenvolvimento / Start dev server
pnpm dev
# Aplicação disponível em / App available at: http://localhost:5173
```

---

## Testes / Tests

### Backend

```bash
cd tinno2
dotnet test Tinno2.Tests/Tinno2.Tests.csproj --verbosity normal
# 21 testes / 21 tests
```

### Frontend

```bash
cd tinnoFrontend

# Executar uma vez / Run once
pnpm test:run

# Modo watch / Watch mode
pnpm test

# Com cobertura / With coverage
pnpm test:coverage
# 15 testes / 15 tests
```

---

## Estrutura do repositório / Repository structure

```
cnpjQuery/
├── tinno2/                        # Backend (ASP.NET Core 10)
│   ├── Tinno2.slnx                # Solution file
│   ├── Tinno2/                    # Projeto principal / Main project
│   │   ├── Controllers/           # CnpjController
│   │   ├── DTOs/                  # 12 Data Transfer Objects
│   │   ├── Services/              # ReceitaFederalService, SimplesNacionalService
│   │   ├── Program.cs             # DI, CORS, OpenAPI, middleware
│   │   ├── appsettings.json       # Configuração base (token placeholder)
│   │   └── Dockerfile             # Multi-stage Linux build
│   └── Tinno2.Tests/              # Projeto de testes / Test project
│       ├── Controllers/           # CnpjControllerTests (10 testes)
│       └── Services/              # ReceitaFederalServiceTests (5) + SimplesNacionalServiceTests (6)
│
├── tinnoFrontend/                 # Frontend (React + Vite)
│   ├── src/
│   │   ├── api/                   # client.ts, cnpj.ts, schema.d.ts (gerado/generated)
│   │   ├── components/ui/         # Componentes shadcn/ui
│   │   ├── lib/                   # cnpj.ts (formatCnpj), utils.ts
│   │   ├── test/                  # App.test.tsx (7), lib/cnpj.test.ts (8)
│   │   └── App.tsx                # Componente principal / Main component
│   ├── vite.config.ts
│   └── package.json
│
├── .vscode/                       # Tasks e launch configs
├── .gitignore
├── README.md
└── TECHNICAL.md                   # Documentação técnica aprofundada
```

---

## Licença / License

MIT
