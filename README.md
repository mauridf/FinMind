# 🧠 FinMind - Sistema de Gestão Financeira Pessoal

Sistema completo para controle financeiro pessoal com análise preditiva e simulação de investimentos.

## 🚀 Tecnologias

- **Backend**: .NET 9, ASP.NET Core
- **Database**: MongoDB
- **Autenticação**: JWT Tokens
- **Documentação**: Swagger/OpenAPI
- **Testes**: xUnit + Moq

## 📋 Requisitos

- .NET 9.0 SDK
- MongoDB 7.0+

## 🏃‍♂️ Execução Local

### Opção 1: Visual Studio
1. Abra a solution `FinMind.sln`
2. Configure o MongoDB local em `mongodb://localhost:27017`
3. Execute o projeto `FinMind.API`

### Opção 2: CLI
```bash
dotnet run --project FinMind.API
```

## 🔐 Autenticação

Registrar usuário:
```bash
POST /api/auth/register
{
  "email": "usuario@email.com",
  "password": "Senha123!",
  "confirmPassword": "Senha123!",
  "name": "Nome Usuário"
}
```

Fazer login e usar o token JWT nos headers:
```
Authorization: Bearer {seu-token}
```

## 🧪 Testes
```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Endpoints Principais

| Módulo       | Endpoints              | Descrição                 |
|--------------|------------------------|---------------------------|
| Auth         | /api/auth/*            | Autenticação JWT          |
| Transactions | /api/transactions/*    | Gestão de transações      |
| Dashboard    | /api/dashboard/*       | Relatórios e analytics    |
| Goals        | /api/goals/*           | Metas financeiras         |
| Budgets      | /api/budgets/*         | Orçamentos                |

## 📈 Estrutura do Projeto
```
FinMind/
├── FinMind.API/          # API Layer
├── FinMind.Application/  # Business Logic
├── FinMind.Domain/       # Domain Models
├── FinMind.Infrastructure/ # Data Access
└── FinMind.Tests/        # Unit Tests
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

MIT License - veja o arquivo LICENSE para detalhes.
