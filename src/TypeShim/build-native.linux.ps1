# Build NativeAOT artifacts for Linux RIDs using Docker.
# Requires Docker.

param(
    [Parameter(Mandatory = $true)]
    [string]$RID
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..") | Select-Object -ExpandProperty Path

$muslRids = @(
    "linux-musl-x64",
    "linux-musl-arm64"
)

if ($muslRids -contains $RID) {
    $image = "mcr.microsoft.com/dotnet/sdk:10.0-alpine"
    $install = "apk add --no-cache powershell git ca-certificates"
} else {
    $image = "mcr.microsoft.com/dotnet/sdk:10.0"
    $install = "apt-get update && apt-get install -y --no-install-recommends powershell git ca-certificates && rm -rf /var/lib/apt/lists/*"
}

Write-Host "Building RID $RID using Docker image $image" -ForegroundColor Cyan

docker run --rm `
    -v "${RepoRoot}:/repo" `
    -w /repo `
    $image `
    sh -lc "$install && pwsh -NoProfile -File /repo/src/TypeShim/build-native.ps1 -RID $RID"
