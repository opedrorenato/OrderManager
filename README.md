# Order Manager

Solução completa para gerenciamento e processamento de ordens usando protocolo FIX 4.4, com backend em C# (.NET 8) e frontend em Angular.

## Descrição

Solução composta por três aplicações principais que se comunicam usando o protocolo FIX com a versão 4.4 (lib do QuickFix disponível em https://quickfixn.org/):

1. **OrderGenerator** (C# API) - Aplicação que expõe endpoints REST para criar novas ordens
2. **OrderAccumulator** (C# FIX Servidor) - Processa ordens via FIX e calcula exposição financeira
3. **Frontend** (Angular) - Interface web para criar ordens e visualizar exposição

## Tecnologias Utilizadas

- **Backend**: C# 12, .NET 8, ASP.NET Core
- **Protocolo**: FIX 4.4 (QuickFixN)
- **Frontend**: Angular 17, TypeScript, Bootstrap
- **Containerização**: Docker & Docker Compose

## Fluxo de Comunicação

```
Frontend (Angular)
    ↓
OrderGenerator (API REST - C#)
    ↓
FIX Protocol 4.4
    ↓
OrderAccumulator (FIX Server - C#)
    ↓
Exposição Calculada & Armazenada
```

## Funcionalidades

### OrderGenerator

- API REST para criar novas ordens (NewOrderSingle)
- Validação de campos:
  - **Símbolo**: PETR4, VALE3 ou VIIA4
  - **Lado**: COMPRA ou VENDA
  - **Quantidade**: 1 até 99.999
  - **Preço**: Múltiplo de 0,01, máximo 999,99
- Comunicação FIX com OrderAccumulator

### OrderAccumulator

- Servidor FIX que recebe ordens do OrderGenerator
- Calcula exposição financeira consolidada por símbolo
- API REST para consultar:
  - Exposição total por símbolo
  - Todas as ordens processadas
  - Resumo de compras e vendas

### Frontend

- Formulário intuitivo para criar ordens
- Visualização em tempo real da exposição financeira
- Dashboard com histórico de ordens

## Instalação e Uso

### Pré-requisitos

- Docker Desktop instalado
- Git
- (Opcional) .NET 8 SDK, Node.js 20+ para desenvolvimento local

### Com Docker Compose (Recomendado)

1. **Clone o repositório**

   ```bash
   git clone <repository-url>
   cd OrderManager
   ```

2. **Inicie os containers**

   ```bash
   docker-compose up --build
   ```

   Aguarde até ver as mensagens indicando que os serviços estão rodando.

3. **Acesse a aplicação**
   - Frontend: http://localhost:4200
   - OrderGenerator Swagger: http://localhost:5000/swagger
   - OrderAccumulator: Console

4. **Parar os containers**
   ```bash
   docker-compose down
   ```

### Desenvolvimento Local (sem Docker)

#### OrderGenerator

```bash
cd src/OrderGenerator
dotnet restore
dotnet run --launch-profile https
```

Acesso: http://localhost:5000

#### OrderAccumulator

```bash
cd src/OrderAccumulator
dotnet restore
dotnet run --launch-profile https
```

Acesso: http://localhost:5001

#### Frontend

```bash
cd src/Frontend
npm install
ng serve
```

Acesso: http://localhost:4200

## Estrutura do Projeto

```
OrderManager/
├── src/
│   ├── OrderGenerator/           # API REST para criar ordens
│   │   ├── Controllers/
│   │   ├── FIXClientService.cs   # Cliente FIX
│   │   ├── Models/
│   │   └── Program.cs
│   ├── OrderAccumulator/         # Servidor FIX & API de exposição
│   │   ├── Controllers/
│   │   ├── FIXServerService.cs   # Servidor FIX
│   │   ├── ExposureService.cs    # Cálculo de exposição
│   │   └── Program.cs
│   └── Frontend/                 # Angular Application
│       ├── src/app/
│       │   ├── components/
│       │   ├── services/
│       │   └── app.component.ts
│       └── package.json
├── .docker/
│   ├── Dockerfile.OrderGenerator
│   ├── Dockerfile.OrderAccumulator
│   └── Dockerfile.Frontend
├── docker-compose.yml
└── README.md
```

## Exemplo de Uso

1. Abra http://localhost:4200 no navegador
2. Na aba "Nova Ordem", preencha os campos:
   - Símbolo: PETR4
   - Lado: COMPRA
   - Quantidade: 1000
   - Preço: 25.50
3. Clique em "Enviar Ordem"
4. Acesse a aba "Exposição" para visualizar a exposição financeira

## Cálculo de Exposição

Para cada símbolo:

```
Exposição = (Σ Preço × Quantidade COMPRA) - (Σ Preço × Quantidade VENDA)
```

**Exemplo:**

- COMPRA: 100 unidades @ 25,50 = R$ 2.550,00
- COMPRA: 50 unidades @ 26,00 = R$ 1.300,00
- VENDA: 30 unidades @ 25,75 = R$ 772,50

Exposição = (2.550 + 1.300) - 772,50 = **R$ 3.077,50**

---

This is a challenge by [Coodesh](https://coodesh.com/)
