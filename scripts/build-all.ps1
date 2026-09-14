Write-Host "Iniciando compilacao de todos os projetos .NET 10 do laboratorio..." -ForegroundColor Cyan

$net10Projects = Get-ChildItem -Path "concepts" -Recurse -Filter "*.csproj" | Where-Object { $_.DirectoryName -like "*\net10*" }

$total = $net10Projects.Count
$sucessos = 0
$falhas = 0

Write-Host "Encontrados $total projetos .NET 10 para compilar." -ForegroundColor Gray

foreach ($proj in $net10Projects) {
    Write-Host "Compilando: $($proj.Name)..." -NoNewline
    $output = dotnet build $proj.FullName -c Release --nologo -v q 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " [OK]" -ForegroundColor Green
        $sucessos++
    } else {
        Write-Host " [FALHOU]" -ForegroundColor Red
        Write-Host $output -ForegroundColor DarkRed
        $falhas++
    }
}

Write-Host ""
Write-Host "Resultado da Compilacao Geral:" -ForegroundColor Cyan
Write-Host "Total: $total | Sucessos: $sucessos | Falhas: $falhas" -ForegroundColor $(if ($falhas -eq 0) { "Green" } else { "Red" })

if ($falhas -gt 0) {
    exit 1
}
