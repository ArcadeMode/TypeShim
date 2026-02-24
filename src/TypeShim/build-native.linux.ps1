# Build NativeAOT artifacts for Linux RIDs using Docker.
# Requires Docker.

param(
    [Parameter(Mandatory = $true)]
    [string]$RID
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "../..") | Select-Object -ExpandProperty Path

$muslRids = @(
    "linux-musl-x64",
    "linux-musl-arm64"
)

# Read required SDK version from src/global.json (repo-root global.json)
$globalJsonPath = Join-Path $RepoRoot "src/global.json"
if (!(Test-Path $globalJsonPath)) {
    throw "global.json not found at '$globalJsonPath'"
}
$sdkVersion = (Get-Content $globalJsonPath -Raw | ConvertFrom-Json).sdk.version
if ([string]::IsNullOrWhiteSpace($sdkVersion)) {
    throw "Failed to read sdk.version from '$globalJsonPath'"
}

$image = if ($muslRids -contains $RID) {
    "mcr.microsoft.com/dotnet-buildtools/prereqs:alpine-3.20"
} else {
    "mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-22.04"
}

Write-Host "Preparing binfmt for multi-architecture builds" -ForegroundColor Cyan
docker run --privileged --rm tonistiigi/binfmt --install all

Write-Host "Building RID $RID using Docker image $image (SDK required: $sdkVersion)" -ForegroundColor Cyan

function Get-DockerPlatformForRid([string]$rid) {
    switch -Wildcard ($rid) {
        "linux-x64" { return "linux/amd64" }
        "linux-musl-x64" { return "linux/amd64" }

        "linux-arm64" { return "linux/arm64" }
        "linux-musl-arm64" { return "linux/arm64" }

        "linux-arm" { return "linux/arm/v7" }
        "linux-musl-arm" { return "linux/arm/v7" }

        default { return "" }
    }
}

$platform = Get-DockerPlatformForRid $RID
if ([string]::IsNullOrWhiteSpace($platform)) {
    throw "Unknown/unsupported RID '$RID' for platform mapping."
}

Write-Host "Docker platform: $platform" -ForegroundColor DarkCyan

# Run publish directly (no PowerShell in-container), and ensure the requested SDK exists (global.json compliance).
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

# Enable pipefail if the shell supports it (dash doesn't).
(set -o pipefail) 2>/dev/null && set -o pipefail || true

SDK_VERSION="${TYPESHIM_SDK_VERSION}"
RID="${TYPESHIM_RID}"

echo "Installing .NET SDK ${SDK_VERSION} to satisfy global.json..."

# Ensure curl + bash exist (dotnet-install.sh is bash)
if command -v apk >/dev/null 2>&1; then
  apk add --no-cache bash curl ca-certificates
else
  apt-get update
  apt-get install -y --no-install-recommends bash curl ca-certificates
  rm -rf /var/lib/apt/lists/*
fi

curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --version "${SDK_VERSION}" --install-dir /tmp/dotnet
export PATH="/tmp/dotnet:${PATH}"

echo "Using dotnet:"
dotnet --info

OUTDIR="/repo/src/TypeShim/bin/pack/build/${RID}"
mkdir -p "$OUTDIR"

dotnet publish "/repo/src/TypeShim.Generator/TypeShim.Generator.csproj" \
  -c Release \
  -o "$OUTDIR" \
  /p:NativeMode=true \
  -r "$RID"
'@