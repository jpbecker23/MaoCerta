# Mão Certa

Plataforma para conectar clientes a profissionais de serviços residenciais. O back-end expõe uma API segura com autenticação JWT, gestão de usuários, categorias, solicitações de serviço e avaliações. A interface web em ASP.NET MVC consome essa API e o script `rodar-projeto.ps1` sobe tudo de uma vez em ambiente local.

## Stack
- .NET 8 (ASP.NET Core Web API + MVC)
- Entity Framework Core 8 + PostgreSQL
- ASP.NET Identity com JWT Bearer
- Serilog para logs e Swagger para documentação interativa

## Estrutura do repositório
- `MaoCerta.API/` – Web API com controllers (auth, clientes, profissionais, solicitações, avaliações, categorias) e migrations EF Core.
- `MaoCerta.Web/` – Interface MVC que consome a API (`API_BASE_URL`), com cache e assets em `wwwroot`.
- `MaoCerta.Domain/`, `MaoCerta.Application/`, `MaoCerta.Infrastructure/` – Camadas de domínio, serviços e acesso a dados (repositórios e `ApplicationDbContext`).
- `Migrations/` – Migrations antigas do protótipo Razor Pages.
- `rodar-projeto.ps1` – Script que inicia API e Web em janelas separadas do PowerShell.
- `env.example` – Modelo de variáveis de ambiente usadas pela API e pela web.

## Pré-requisitos
- .NET 8 SDK
- PostgreSQL 15+ em execução
- PowerShell 5+ (para usar o script de inicialização)
- (Opcional) Ferramenta `dotnet-ef`: `dotnet tool install --global dotnet-ef`

## Configuração
1) Clone o repositório e entre na pasta.
2) Crie o arquivo `.env` a partir do modelo:
   ```powershell
   Copy-Item env.example .env
   ```
   Ajuste as chaves:
   - `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USERNAME`, `DB_PASSWORD`
   - `JWT_SECRET_KEY` (mínimo 32 caracteres), `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`
3) Crie o banco de dados no PostgreSQL (exemplo):
   ```sql
   CREATE DATABASE maocerta;
   ```
4) Restaure dependências:
   ```powershell
   dotnet restore MaoCerta.sln
   ```
5) Aplique as migrations do back-end (opcional, pois o projeto executa `Database.Migrate()` na subida):
   ```powershell
   dotnet ef database update --project MaoCerta.API --startup-project MaoCerta.API
   ```

## Como executar
- Opção rápida (API + Web):
  ```powershell
  powershell -ExecutionPolicy Bypass -File .\rodar-projeto.ps1 `
    -ApiUrls "https://localhost:7001;http://localhost:5083" `
    -WebUrls "https://localhost:7080;http://localhost:5088"
  ```
  API: `https://localhost:7001/swagger` • Web: `https://localhost:7080`

- Executar manualmente:
  ```powershell
  # API
  $env:ASPNETCORE_URLS="https://localhost:7001;http://localhost:5083"
  dotnet run --project MaoCerta.API

  # Interface Web (aponta para a API)
  $env:API_BASE_URL="https://localhost:7001/api"
  $env:ASPNETCORE_URLS="https://localhost:7080;http://localhost:5088"
  dotnet run --project MaoCerta.Web
  ```

## Principais recursos da API
- Autenticação e registro de usuários com JWT (`/api/auth/register` e `/api/auth/login`).
- CRUD de clientes e profissionais, vinculados a categorias de serviço.
- Cadastro e consulta de categorias.
- Solicitações de serviço com vínculo cliente/profissional e atualização de status.
- Avaliações com notas agregadas (qualidade, preço, tempo, comunicação, profissionalismo).
- Health check em `/api/health` e documentação via Swagger.

## Dicas e resolução de problemas
- Erro de conexão: revise `.env` e confirme que o serviço PostgreSQL está em execução e acessível.
- `dotnet ef` não encontrado: feche e reabra o terminal após instalar a ferramenta ou adicione `$env:USERPROFILE\.dotnet\tools` ao `PATH`.
- Certificados HTTPS de desenvolvimento: se o navegador alertar, confie no certificado do ASP.NET ou acesse via HTTP (urls configuradas acima).

## Próximos passos
- Adicionar testes de integração para os controllers da API.
- Completar validadores FluentValidation no projeto de aplicação.
- Publicar o front-end em ambiente público apontando para uma instância hospedada da API.
