# Build TypeShim.Generator for NativeAOT for a specific RID

param(
    [Parameter(Mandatory = $true)]
    [string]$RID
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $ScriptDir "..\..") | Select-Object -ExpandProperty Path
$GeneratorProject = Join-Path $ProjectRoot "src\TypeShim.Generator\TypeShim.Generator.csproj"

$OutputDir = Join-Path $ScriptDir ".\bin\pack\build\$RID"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host ""
Write-Host "Building Generator (NativeAOT) RID=$RID" -ForegroundColor Yellow

dotnet publish $GeneratorProject -c Release -o $OutputDir /p:NativeMode=true -r $RID

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Output: $OutputDir"
