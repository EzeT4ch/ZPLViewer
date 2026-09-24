param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    dotnet restore ZplViewer.sln --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
    dotnet build ZplViewer.sln -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    dotnet test tests/ZplViewer.Tests/ZplViewer.Tests.csproj -c $Configuration --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    $package = Join-Path $repoRoot "src/ZplViewer.Extension/bin/$Configuration/net8.0-windows8.0/ZplViewer.Extension.vsix"
    $artifacts = Join-Path $repoRoot 'artifacts'
    New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
    Copy-Item -LiteralPath $package -Destination (Join-Path $artifacts 'ZplViewer.vsix')
    Get-FileHash -LiteralPath (Join-Path $artifacts 'ZplViewer.vsix') -Algorithm SHA256
} finally { Pop-Location }
