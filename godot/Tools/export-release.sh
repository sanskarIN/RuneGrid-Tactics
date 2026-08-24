#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GODOT_BIN="${GODOT4:-godot}"
TARGET="${1:-windows}"

case "$TARGET" in
  windows) PRESET="Windows Desktop"; OUTPUT="build/windows/RuneGridTactics.exe" ;;
  linux) PRESET="Linux/X11"; OUTPUT="build/linux/RuneGridTactics.x86_64" ;;
  android) PRESET="Android"; OUTPUT="build/android/RuneGridTactics.apk" ;;
  *) echo "Usage: $0 [windows|linux|android]" >&2; exit 64 ;;
esac

mkdir -p "$PROJECT_DIR/$(dirname "$OUTPUT")"
"$GODOT_BIN" --headless --path "$PROJECT_DIR" --export-release "$PRESET" "$OUTPUT"
echo "Export complete: $PROJECT_DIR/$OUTPUT"
