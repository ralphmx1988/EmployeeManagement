Write-Host "🚀 Starting Employee Management Application..." -ForegroundColor Green
Write-Host ""

Set-Location "C:\EmployeeManagement\src\EmployeeManagement.Web"

Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "🔨 Building application..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 The application will be available at:" -ForegroundColor Cyan
    Write-Host "   HTTP:  http://localhost:5091" -ForegroundColor White
    Write-Host "   HTTPS: https://localhost:7095" -ForegroundColor White
    Write-Host ""
    Write-Host "⚡ Starting application..." -ForegroundColor Green
    Write-Host "   Press Ctrl+C to stop the application" -ForegroundColor Gray
    Write-Host ""
    
    dotnet run
} else {
    Write-Host ""
    Write-Host "❌ Build failed. Please check the errors above." -ForegroundColor Red
    pause
}
