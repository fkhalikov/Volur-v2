# Volur Development Setup Script (Windows PowerShell)
# This script sets up the development environment for Volur

$ErrorActionPreference = "Stop"

Write-Host "🚀 Setting up Volur development environment..." -ForegroundColor Green

# Check prerequisites
function Test-Command {
    param($Command)
    
    if (!(Get-Command $Command -ErrorAction SilentlyContinue)) {
        Write-Host "❌ $Command is not installed. Please install it first." -ForegroundColor Red
        exit 1
    }
}

Write-Host "📋 Checking prerequisites..." -ForegroundColor Cyan
Test-Command "dotnet"
Test-Command "node"
Test-Command "docker"

Write-Host "✅ All prerequisites are installed" -ForegroundColor Green

# Backend setup
Write-Host ""
Write-Host "🔧 Setting up backend..." -ForegroundColor Cyan
Set-Location src\Volur.Api

Write-Host "  - Initializing user secrets..." -ForegroundColor White
dotnet user-secrets init

Write-Host "  - Please enter your EODHD API token:" -ForegroundColor Yellow
$ApiToken = Read-Host
dotnet user-secrets set "Eodhd:ApiToken" $ApiToken

Write-Host "  - Restoring NuGet packages..." -ForegroundColor White
Set-Location ..\..
dotnet restore

Write-Host "✅ Backend setup complete" -ForegroundColor Green

# Frontend setup
Write-Host ""
Write-Host "🎨 Setting up frontend..." -ForegroundColor Cyan
Set-Location web\volur-web

Write-Host "  - Installing npm packages..." -ForegroundColor White
npm install

Write-Host "✅ Frontend setup complete" -ForegroundColor Green

# MongoDB setup
Write-Host ""
Write-Host "🗄️  Starting MongoDB with Docker..." -ForegroundColor Cyan
Set-Location ..\..

$mongoRunning = docker ps -q -f name=volur-mongo
if (!$mongoRunning) {
    $mongoExists = docker ps -aq -f status=exited -f name=volur-mongo
    if ($mongoExists) {
        Write-Host "  - Starting existing MongoDB container..." -ForegroundColor White
        docker start volur-mongo
    } else {
        Write-Host "  - Creating new MongoDB container..." -ForegroundColor White
        docker run -d --name volur-mongo -p 27017:27017 mongo:7.0
    }
}

Write-Host "✅ MongoDB is running" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "🎉 Development environment setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To start developing:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Start the API:" -ForegroundColor White
Write-Host "   cd src\Volur.Api" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host "   (API will be at http://localhost:5000)" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. Start the frontend (in a new terminal):" -ForegroundColor White
Write-Host "   cd web\volur-web" -ForegroundColor Gray
Write-Host "   npm run dev" -ForegroundColor Gray
Write-Host "   (Web UI will be at http://localhost:5173)" -ForegroundColor Yellow
Write-Host ""
Write-Host "3. Access Swagger documentation:" -ForegroundColor White
Write-Host "   http://localhost:5000/swagger" -ForegroundColor Blue
Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green

