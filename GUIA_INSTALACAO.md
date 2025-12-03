# Guia de InstalaÃ§Ã£o - Projeto MÃ£o Certa

## ðŸ“‹ PrÃ©-requisitos

Antes de comeÃ§ar, instale os seguintes softwares:

1. **.NET 8 SDK**: <https://dotnet.microsoft.com/download/dotnet/8.0>
2. **PostgreSQL 15+**: <https://www.postgresql.org/download/windows/>
3. **Node.js 18+**: <https://nodejs.org/>
4. **Git** (opcional): <https://git-scm.com/downloads>

## ðŸ”§ Passo 1: Instalar PostgreSQL

1. Baixe e instale o PostgreSQL do site oficial: <https://www.postgresql.org/download/windows/>
2. Durante a instalaÃ§Ã£o, **anote a senha** que vocÃª configurar para o usuÃ¡rio `postgres`
3. Deixe a porta padrÃ£o: `5432`
4. Instale tambÃ©m o **pgAdmin 4** (vem junto na instalaÃ§Ã£o)
5. ApÃ³s a instalaÃ§Ã£o, verifique se o serviÃ§o estÃ¡ rodando:
   - Pressione `Win + R`
   - Digite: `services.msc`
   - Procure por **"postgresql"** e verifique se estÃ¡ **"Em execuÃ§Ã£o"**
   - Se nÃ£o estiver, clique com botÃ£o direito â†’ **"Iniciar"**

## ðŸ—„ï¸ Passo 2: Criar Banco de Dados

### 2.1. Abrir pgAdmin 4

1. Procure por "pgAdmin 4" no menu Iniciar
2. Abra o pgAdmin 4

### 2.2. Conectar ao Servidor

1. Clique com botÃ£o direito em **"Servers"** â†’ **"Create"** â†’ **"Server..."**
2. **Aba "General"**:
   - **Name**: `MaoCerta Local`
3. **Aba "Connection"**:
   - **Host name/address**: `localhost`
   - **Port**: `5432`
   - **Maintenance database**: `postgres`
   - **Username**: `postgres`
   - **Password**: `[sua senha do PostgreSQL]`
   - âœ… Marque **"Save password"**
4. Clique em **"Save"**

### 2.3. Criar o Banco de Dados

1. Expanda o servidor criado
2. Clique com botÃ£o direito em **"Databases"** â†’ **"Create"** â†’ **"Database..."**
3. **Aba "General"**:
   - **Database**: `maocerta` (minÃºsculo, sem espaÃ§os)
   - **Owner**: `postgres`
4. Clique em **"Save"**

## ðŸ“¦ Passo 3: Clonar/Baixar o Projeto e Verificar PrÃ©-requisitos

1. Baixe ou clone o projeto
2. Extraia em uma pasta (se for ZIP)
3. Abra o PowerShell na pasta **raiz do projeto** (onde estÃ¡ o arquivo `GUIA_INSTALACAO.md`)

### 3.1. Verificar InstalaÃ§Ãµes

Verifique se todos os prÃ©-requisitos estÃ£o instalados:

```powershell
# Verificar .NET 8 SDK
dotnet --version
# Deve mostrar: 8.x.x ou superior

# Verificar Node.js
node --version
# Deve mostrar: v18.x.x ou superior

# Verificar npm
npm --version
# Deve mostrar: 9.x.x ou superior

# Verificar PostgreSQL (via serviÃ§o)
Get-Service -Name postgresql* -ErrorAction SilentlyContinue
# Deve listar o serviÃ§o PostgreSQL
```

**Se algum comando falhar, instale o software correspondente dos links no inÃ­cio do guia.**

### 3.2. Verificar Estrutura do Projeto

```powershell
# Verificar diretÃ³rios principais
Test-Path "MaoCerta.API"
Test-Path "frontend"
# Ambos devem retornar True

# Verificar arquivos importantes
Test-Path "MaoCerta.API/MaoCerta.API.csproj"
Test-Path "frontend/package.json"
Test-Path "executar-migracoes.ps1"
Test-Path "rodar-projeto.ps1"
# Todos devem retornar True
```

### 3.3. Restaurar Pacotes do Backend

```powershell
cd MaoCerta.API
dotnet restore
cd ..
```

Isso instalarÃ¡ todas as dependÃªncias NuGet necessÃ¡rias automaticamente.

## �sT��? Passo 4: Configurar Banco de Dados

### 4.1. Criar arquivo .env (Obrigat��rio)

Todas as credenciais sensǭveis agora ficam somente no arquivo `.env` na raiz do projeto. Sem ele, a API e o site nǜo conseguem se conectar ao banco.

1. Copie o arquivo `env.example` para `.env` na raiz do projeto:

   ```powershell
   Copy-Item env.example .env
   ```

2. Edite o arquivo `.env` e configure:

   ```
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=maocerta
   DB_USERNAME=postgres
   DB_PASSWORD=SUA_SENHA_AQUI
   ```

3. Ajuste tambǸm as variǭveis de JWT conforme necessǭrio (use um segredo com pelo menos 32 caracteres):

   ```
   JWT_SECRET_KEY=uma_senha_bem_grande_com_pelo_menos_32_caracteres
   JWT_ISSUER=MaoCertaAPI
   JWT_AUDIENCE=MaoCertaUsers
   JWT_EXPIRY_MINUTES=60
   ```

> **Importante:** Os arquivos `appsettings*.json` nǜo armazenam mais connection strings nem credenciais. Toda altera��ǜo deve ser feita exclusivamente no `.env`.
## ðŸ”¨ Passo 5: Instalar Ferramenta Entity Framework

Abra o PowerShell e execute:

```powershell
dotnet tool install --global dotnet-ef
```

**IMPORTANTE**: Feche e reabra o PowerShell apÃ³s instalar.

## ðŸš€ Passo 6: Executar MigraÃ§Ãµes do Banco

**OpÃ§Ã£o A: Usar o Script (Recomendado)**

```powershell
powershell -ExecutionPolicy Bypass -File .\executar-migracoes.ps1
```

**OpÃ§Ã£o B: Manual**

```powershell
cd MaoCerta.API
dotnet ef database update
```

**Se der erro de "dotnet ef not found":**

```powershell
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
dotnet ef database update
```

## ðŸ“š Passo 7: Instalar DependÃªncias do Frontend

```powershell
cd frontend
npm install
```

Aguarde a instalaÃ§Ã£o terminar (pode levar alguns minutos).

## âœ… Passo 8: Verificar InstalaÃ§Ã£o

### 8.1. Verificar Backend

```powershell
cd MaoCerta.API
dotnet build
```

Deve compilar sem erros. Se houver erros:

- Verifique se o PostgreSQL estÃ¡ rodando
- Verifique se a connection string estÃ¡ correta
- Execute `dotnet restore` novamente

### 8.2. Verificar Frontend

```powershell
cd frontend
npm list --depth=0
```

Deve listar as dependÃªncias instaladas. Se houver problemas:

- Execute `npm install` novamente
- Verifique se o Node.js estÃ¡ atualizado

### 8.3. Build de ProduÃ§Ã£o (Opcional)

Para garantir que tudo estÃ¡ funcionando, teste os builds de produÃ§Ã£o:

```powershell
# Backend
cd MaoCerta.API
dotnet build --configuration Release

# Frontend
cd ..\frontend
npm run build
```

Ambos devem compilar sem erros ou warnings.

## ðŸŽ¯ Passo 9: Rodar o Projeto

**OpÃ§Ã£o A: Script Automatizado (Recomendado)**

```powershell
.\rodar-projeto.ps1
```

**OpÃ§Ã£o B: Manual (2 Terminais)**

**Terminal 1 - Backend:**

```powershell
cd MaoCerta.API
dotnet run --urls "http://localhost:5083"
```

**Terminal 2 - Frontend:**

```powershell
cd frontend
npm start
```

## ðŸŒ Passo 10: Acessar o Sistema

ApÃ³s iniciar os serviÃ§os:

1. **Frontend**: <http://localhost:3000>
2. **Swagger (API)**: <http://localhost:5083/swagger>
3. **Backend API**: <http://localhost:5083/api>

## ðŸ” VerificaÃ§Ã£o Final

### Testar Registro de UsuÃ¡rio

1. Acesse: <http://localhost:3000>
2. Clique em **"Cadastrar"**
3. Preencha:
   - Nome completo
   - Email
   - Senha (mÃ­nimo 6 caracteres)
   - Confirmar senha
   - Telefone (opcional)
4. Clique em **"Cadastrar"**
5. Deve redirecionar para a pÃ¡gina inicial

### Testar Login

1. Clique em **"Entrar"**
2. Digite o email e senha cadastrados
3. Deve fazer login e redirecionar

## âŒ SoluÃ§Ã£o de Problemas Comuns

### Erro: "dotnet ef not found"

**SoluÃ§Ã£o:**

```powershell
dotnet tool install --global dotnet-ef
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
```

Se ainda nÃ£o funcionar, feche e reabra o PowerShell.

### Erro: "password authentication failed"

**SoluÃ§Ã£o:**

1. Verifique se a senha no arquivo `.env` estÃ¡ correta
2. Teste a senha no pgAdmin 4
3. Se necessÃ¡rio, altere a senha do PostgreSQL

### Erro: "database does not exist"

**SoluÃ§Ã£o:**

1. Verifique se o banco `maocerta` foi criado no pgAdmin 4
2. Confirme que o nome estÃ¡ em minÃºsculo: `maocerta`

### Erro: "connection refused"

**SoluÃ§Ã£o:**

1. Verifique se o PostgreSQL estÃ¡ rodando:
   - Pressione `Win + R`
   - Digite: `services.msc`
   - Procure por **"postgresql"**
   - Se nÃ£o estiver rodando, clique com botÃ£o direito â†’ **"Iniciar"**

### Erro: "npm install" falha

**SoluÃ§Ã£o:**

```powershell
cd frontend
rm -r node_modules
rm package-lock.json
npm install
```

### Erro: Porta 5083 ou 3000 jÃ¡ em uso

**SoluÃ§Ã£o:**

1. Use o script para encerrar processos:

   ```powershell
   .\encerrar-porta.ps1
   ```

2. Ou encontre e encerre manualmente:

   ```powershell
   Get-NetTCPConnection -LocalPort 5083 | Select-Object OwningProcess
   Stop-Process -Id <PID>
   ```

3. Ou altere as portas nos arquivos de configuraÃ§Ã£o

## ðŸ“ Checklist de InstalaÃ§Ã£o

- [ ] .NET 8 SDK instalado
- [ ] PostgreSQL instalado e rodando
- [ ] Banco de dados `maocerta` criado no pgAdmin
- [ ] Arquivo `.env` criado e preenchido com as credenciais do banco e JWT
- [ ] Ferramenta `dotnet-ef` instalada globalmente
- [ ] PATH do .NET Tools configurado (ou reiniciado PowerShell)
- [ ] MigraÃ§Ãµes executadas com sucesso
- [ ] DependÃªncias do frontend instaladas (`npm install`)
- [ ] Backend compila sem erros (`dotnet build`)
- [ ] Frontend compila sem erros (`npm run build`)
- [ ] Backend roda na porta 5083
- [ ] Frontend roda na porta 3000
- [ ] Swagger acessÃ­vel em <http://localhost:5083/swagger>
- [ ] Registro de usuÃ¡rio funciona
- [ ] Login funciona
- [ ] CriaÃ§Ã£o de solicitaÃ§Ã£o de serviÃ§o funciona

## ðŸŽ‰ ConcluÃ­do

Se todos os itens do checklist estÃ£o marcados, o projeto estÃ¡ instalado e funcionando!

**PrÃ³ximos passos:**

- Explore o Swagger: <http://localhost:5083/swagger>
- Teste todas as funcionalidades
- Consulte a documentaÃ§Ã£o da API

---

**DÃºvidas?** Verifique os logs:

- Backend: `MaoCerta.API/logs/maocerta-*.txt`
- Frontend: Console do navegador (F12)




