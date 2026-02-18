#!/bin/bash
# Build TypeShim.Generator for both AOT and non-AOT modes
# Usage: ./build-generators.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
GENERATOR_PROJECT="$PROJECT_ROOT/src/TypeShim.Generator/TypeShim.Generator.csproj"
OUTPUT_DIR="$SCRIPT_DIR/../GeneratorBuilds"

echo "Building TypeShim.Generator in both AOT and non-AOT modes..."
echo "Project Root: $PROJECT_ROOT"
echo "Generator Project: $GENERATOR_PROJECT"
echo "Output Directory: $OUTPUT_DIR"

# Clean output directory
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR/NonAOT"
mkdir -p "$OUTPUT_DIR/AOT"

# Build non-AOT version
echo ""
echo "Building non-AOT version..."
dotnet build "$GENERATOR_PROJECT" -c Release -o "$OUTPUT_DIR/NonAOT" /p:PublishAot=false

# Build AOT version
echo ""
echo "Building AOT version..."
dotnet publish "$GENERATOR_PROJECT" -c Release -o "$OUTPUT_DIR/AOT" /p:PublishAot=true

echo ""
echo "Build completed successfully!"
echo "Non-AOT build: $OUTPUT_DIR/NonAOT"
echo "AOT build: $OUTPUT_DIR/AOT"
