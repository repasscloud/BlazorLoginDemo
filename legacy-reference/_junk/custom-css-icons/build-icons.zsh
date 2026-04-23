#!/usr/bin/env zsh
# build-icons.zsh
set -euo pipefail

ICON_DIR="${ICON_DIR:-$PWD/icons}"
OUT="${OUT:-$PWD/icons.css}"
PREFIX="${PREFIX:-ico}"        # class prefix => .ico-<name>
BASE_CLASS="${BASE_CLASS:-icon}"# base utility class
EMBED="${EMBED:-0}"            # 0=file paths, 1=embed as data URI

[[ -d "$ICON_DIR" ]] || { echo "No dir: $ICON_DIR" >&2; exit 1; }

# Base rules: single-color via currentColor, scales with font-size
cat > "$OUT" <<CSS
/* generated from $ICON_DIR */
.$BASE_CLASS{display:inline-block;width:1em;height:1em;vertical-align:-0.125em;background-color:currentColor;-webkit-mask:no-repeat center/contain;mask:no-repeat center/contain}
CSS

# Iterate SVGs
# shellcheck disable=SC2044
for svg in $(find "$ICON_DIR" -type f -name '*.svg' | sort); do
  # class name from filename
  fname="${svg:t}"                               # tail
  base="${fname%.*}"
  name=$(echo "$base" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-')

  if [[ "$EMBED" == "1" ]]; then
    # Embed SVG as base64 (portable mac/GNU)
    if base64 --help 2>&1 | grep -q -- "-w "; then
      data=$(base64 -w0 < "$svg")
    else
      data=$(base64 < "$svg" | tr -d '\n')
    fi
    printf "\n/* %s */\n.%s-%s{-webkit-mask-image:url(\"data:image/svg+xml;base64,%s\");mask-image:url(\"data:image/svg+xml;base64,%s\");}\n" \
      "$fname" "$PREFIX" "$name" "$data" "$data" >> "$OUT"
  else
    # Reference file path relative to web root; adjust if needed
    rel="${svg#$PWD/}"    # strip PWD prefix if present
    printf "\n/* %s */\n.%s-%s{-webkit-mask-image:url(\"/%s\");mask-image:url(\"/%s\");}\n" \
      "$fname" "$PREFIX" "$name" "$rel" "$rel" >> "$OUT"
  fi
done

echo "Wrote $OUT"
