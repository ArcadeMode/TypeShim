# Build NativeAOT artifacts for Linux RIDs using Docker.
# Requires Docker.

param(
    [Parameter(Mandatory = $true)]
    [string]$RID
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "../..") | Select-Object -ExpandProperty Path
$muslRids = @("linux-musl-x64", "linux-musl-arm64")
$globalJsonPath = Join-Path $RepoRoot "src/global.json"
if (!(Test-Path $globalJsonPath)) {
    throw "global.json not found at '$globalJsonPath'"
}

$sdkVersion = (Get-Content $globalJsonPath -Raw | ConvertFrom-Json).sdk.version
if ([string]::IsNullOrWhiteSpace($sdkVersion)) {
    throw "Failed to read sdk.version from '$globalJsonPath'"
}

$image = if ($muslRids -contains $RID) {
    "mcr.microsoft.com/dotnet/sdk:$sdkVersion-alpine3.23-aot"
} else {
    "mcr.microsoft.com/dotnet/sdk:$sdkVersion-noble-aot"
}

function Get-DockerPlatformForRid([string]$rid) {
    switch -Wildcard ($rid) {
        "linux-x64" { return "linux/amd64" }
        "linux-musl-x64" { return "linux/amd64" }
        "linux-arm64" { return "linux/arm64" }
        "linux-musl-arm64" { return "linux/arm64" }
        default { throw "Unsupported RID '$rid' for platform mapping." }
    }
}

$platform = Get-DockerPlatformForRid $RID
Write-Host "Docker platform: $platform" -ForegroundColor DarkCyan

Write-Host "Building RID $RID using Docker image $image (SDK required: $sdkVersion)" -ForegroundColor Cyan
docker run --rm `
    --platform $platform `
    -e "TYPESHIM_SDK_VERSION=$sdkVersion" `
    -e "TYPESHIM_RID=$RID" `
    -e "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1" `
    -e "DOTNET_CLI_TELEMETRY_OPTOUT=1" `
    -e "DOTNET_MULTILEVEL_LOOKUP=0" `
    -v "${RepoRoot}:/repo" `
    -w /repo `
    $image `
    sh -lc @'
set -eu
(set -o pipefail) 2>/dev/null && set -o pipefail || true

SDK_VERSION="${TYPESHIM_SDK_VERSION}"
RID="${TYPESHIM_RID}"

OUTDIR="/repo/src/TypeShim/bin/pack/build/${RID}"
mkdir -p "$OUTDIR"

dotnet publish "/repo/src/TypeShim.Generator/TypeShim.Generator.csproj" \
  -c Release \
  -o "$OUTDIR" \
  /p:NativeMode=true \
  -r "$RID"
'@