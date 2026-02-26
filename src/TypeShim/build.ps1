param()

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $ScriptDir "..\..") | Select-Object -ExpandProperty Path
$GeneratorProject = Join-Path $ProjectRoot "src\TypeShim.Generator\TypeShim.Generator.csproj"
$AnalyzersProject = Join-Path $ProjectRoot "src\TypeShim.Analyzers\TypeShim.Analyzers.csproj"
$TypeShimProject = Join-Path $ProjectRoot "src\TypeShim\TypeShim.csproj"
$OutputDir = Join-Path $ScriptDir ".\bin\pack"

if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force -Path "$OutputDir\analyzers" | Out-Null
New-Item -ItemType Directory -Force -Path "$OutputDir\build" | Out-Null

Write-Host ""
Write-Host "Building Generator (JIT)" -ForegroundColor Yellow
dotnet publish $GeneratorProject -c Release -o "$OutputDir\build" /p:NativeMode=false
Write-Host ""
Write-Host "Building Analyzers" -ForegroundColor Yellow
dotnet publish $AnalyzersProject -c Release -o "$OutputDir\analyzers"
Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
