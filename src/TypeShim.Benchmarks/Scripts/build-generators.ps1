# Build TypeShim.Generator for both AOT and non-AOT modes
# Usage: .\build-generators.ps1

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $ScriptDir "..\..\..") | Select-Object -ExpandProperty Path
$GeneratorProject = Join-Path $ProjectRoot "src\TypeShim.Generator\TypeShim.Generator.csproj"
$OutputDir = Join-Path $ScriptDir "..\GeneratorBuilds"

Write-Host "Building TypeShim.Generator in both AOT and non-AOT modes..." -ForegroundColor Cyan
Write-Host "Project Root: $ProjectRoot"
Write-Host "Generator Project: $GeneratorProject"
Write-Host "Output Directory: $OutputDir"

# Clean output directory
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force -Path "$OutputDir\NonAOT" | Out-Null
New-Item -ItemType Directory -Force -Path "$OutputDir\AOT" | Out-Null

# Build non-AOT version
Write-Host ""
Write-Host "Building non-AOT version..." -ForegroundColor Yellow
dotnet publish $GeneratorProject -c Release -o "$OutputDir\NonAOT" /p:NativeMode=false

# Build AOT version
Write-Host ""
Write-Host "Building AOT version..." -ForegroundColor Yellow
dotnet publish $GeneratorProject -c Release -o "$OutputDir\AOT" /p:NativeMode=true

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Non-AOT build: $OutputDir\NonAOT"
Write-Host "AOT build: $OutputDir\AOT"
