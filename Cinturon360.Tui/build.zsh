#!/usr/bin/env zsh
set -euo pipefail

# === Build settings ===
RID="osx-arm64"                 # or "osx-x64" for Intel
CONFIG="Release"
OUTPUT_DIR="./bin/Output/$RID"

# === App bundle settings ===
APP_NAME="Cinturon360 Tui"
BINARY_NAME="Cinturon360.Tui"            # actual .NET binary
BINARY_PATH="$OUTPUT_DIR/$BINARY_NAME"

echo "Cleaning output directory..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "Publishing .NET app ($RID, $CONFIG)..."
dotnet publish \
  -c "$CONFIG" \
  -r "$RID" \
  -o "$OUTPUT_DIR" \
  -p:PublishAot=false \
  -p:SelfContained=true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false

# === Sanity check ===
if [[ ! -f "$BINARY_PATH" ]]; then
  echo "ERROR: Binary not found at: $BINARY_PATH"
  exit 1
fi

# === Make the binary executable ===
echo "Making the binary executable..."
chmod +x "$BINARY_PATH"
