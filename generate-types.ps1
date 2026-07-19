$ErrorActionPreference = "Stop"

Write-Host "Rebuilding HomeBot .NET Backend and extracting OpenAPI document..." -ForegroundColor Cyan
dotnet build "$PSScriptRoot\homebot\homebot.csproj"

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet build failed!"
    exit $LASTEXITCODE
}

Write-Host "`nGenerating TanStack Query hooks and TypeScript schemas in web..." -ForegroundColor Cyan
Push-Location "$PSScriptRoot\web"
try {
    pnpm run generate:api
}
finally {
    Pop-Location
}

Write-Host "`nTypes generated successfully!" -ForegroundColor Green
