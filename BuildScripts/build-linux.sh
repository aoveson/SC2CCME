#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo "Usage: $0 <version> [linux-x64|linux-arm64]" >&2
    exit 2
fi

VERSION="${1#v}"
RID="${2:-linux-x64}"
if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?(-[0-9A-Za-z.-]+)?(\+[0-9A-Za-z.-]+)?$ ]]; then
    echo "Version must be a 3- or 4-part version, for example 1.0.0.42." >&2
    exit 2
fi
case "$RID" in
    linux-x64|linux-arm64) ;;
    *) echo "Linux packaging requires a linux-* RID, got: $RID" >&2; exit 2 ;;
esac
if ! command -v mksquashfs >/dev/null 2>&1; then
    echo "Linux AppImage packaging requires mksquashfs. Install squashfs-tools (for example: sudo apt install squashfs-tools)." >&2
    exit 1
fi

VPK_VERSION="1.2.161"
PACK_ID="SC2CCME"
APP_NAME="SC2CCME"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPOSITORY_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPOSITORY_ROOT/ModManager.StarCraft.Application/ModManager.StarCraft.Application.csproj"
ARTIFACT_DIRECTORY="$SCRIPT_DIR/artifacts/$RID"
PUBLISH_DIRECTORY="$ARTIFACT_DIRECTORY/publish"
RELEASE_DIRECTORY="$ARTIFACT_DIRECTORY/releases"
VPK_TOOL_DIRECTORY="$(mktemp -d)"
trap 'rm -rf "$VPK_TOOL_DIRECTORY"' EXIT

rm -rf "$ARTIFACT_DIRECTORY"
mkdir -p "$PUBLISH_DIRECTORY" "$RELEASE_DIRECTORY"

dotnet publish "$PROJECT_PATH" \
    --configuration Release \
    --runtime "$RID" \
    --self-contained true \
    --output "$PUBLISH_DIRECTORY" \
    "-p:Version=$VERSION"

dotnet tool install vpk --tool-path "$VPK_TOOL_DIRECTORY" --version "$VPK_VERSION"
"$VPK_TOOL_DIRECTORY/vpk" pack \
    --packId "$PACK_ID" \
    --packVersion "$VERSION" \
    --packTitle "$APP_NAME" \
    --packDir "$PUBLISH_DIRECTORY" \
    --outputDir "$RELEASE_DIRECTORY" \
    --mainExe "$APP_NAME"

echo "Velopack packages created in $RELEASE_DIRECTORY"