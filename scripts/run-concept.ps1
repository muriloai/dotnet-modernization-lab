param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$Concept,

    [Parameter(Mandatory = $false, Position = 1)]
    [ValidateSet("net10", "framework")]
    [string]$Side = "net10"
)

$conceptStr = "{0:D2}" -f $Concept
$pattern = "concepts/$conceptStr-*"
$matches = Get-ChildItem -Path "concepts" -Directory -Filter "$conceptStr-*"

if ($matches.Count -eq 0) {
    Write-Error "Conceito $Concept nao encontrado em concepts/."
    exit 1
}

$conceptFolder = $matches[0].FullName
$targetFolder = Join-Path $conceptFolder $Side

if (-not (Test-Path $targetFolder)) {
    Write-Error "Pasta $Side nao encontrada em $conceptFolder."
    exit 1
}

Write-Host "Iniciando Conceito $Concept ($Side)..." -ForegroundColor Cyan
Write-Host "Diretorio: $targetFolder" -ForegroundColor Gray

if ($Side -eq "net10") {
    Push-Location $targetFolder
    try {
        dotnet run
    } finally {
        Pop-Location
    }
} else {
    Write-Host "Projetos em .NET Framework 4.8.1 devem ser abertos e executados no Visual Studio 2022 com IIS Express." -ForegroundColor Yellow
    Write-Host "Localizacao do projeto legado: $targetFolder" -ForegroundColor Yellow
}
