# Script para executar toda a plataforma Mao Certa (API + Web MVC)
param(
    [string]$ApiUrls = "https://localhost:7001;http://localhost:5083",
    [string]$WebUrls = "https://localhost:7080;http://localhost:5088"
)

Write-Host "=== Inicializando Plataforma Mao Certa ===" -ForegroundColor Cyan
Write-Host "API URLs: $ApiUrls" -ForegroundColor DarkGray
Write-Host "Web URLs: $WebUrls" -ForegroundColor DarkGray

function Start-ServiceProcess {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )

    Write-Host $Title -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& { Set-Location '$WorkingDirectory'; $Command }"
}

# Executa API
$apiCommand = "`$env:ASPNETCORE_URLS='$ApiUrls'; dotnet run"
Start-ServiceProcess -Title "`n[1/2] Iniciando API..." -WorkingDirectory "$PSScriptRoot\MaoCerta.API" -Command $apiCommand
Write-Host "API disponível em: $ApiUrls" -ForegroundColor Green
Write-Host "Swagger: https://localhost:7001/swagger" -ForegroundColor Cyan

Start-Sleep -Seconds 2

# Executa Web MVC
$webCommand = "`$env:ASPNETCORE_URLS='$WebUrls'; `$env:API_BASE_URL='https://localhost:7001/api'; dotnet run"
Start-ServiceProcess -Title "`n[2/2] Iniciando Interface Web..." -WorkingDirectory "$PSScriptRoot\MaoCerta.Web" -Command $webCommand
Write-Host "Interface Web disponível em: $WebUrls" -ForegroundColor Green

Write-Host "`n=== Serviços em execução ===" -ForegroundColor Green
Write-Host "API:     $ApiUrls" -ForegroundColor White
Write-Host "Web:     $WebUrls" -ForegroundColor White
Write-Host "Swagger: https://localhost:7001/swagger" -ForegroundColor White
Write-Host "`nUse Ctrl+C nas janelas abertas para encerrar cada serviço." -ForegroundColor Yellow
