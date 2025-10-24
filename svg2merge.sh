#!/usr/bin/env bash
set -euo pipefail

dir="./icons/icons"

# Robust to spaces and many files
find "$dir" -type f -name '*.svg' -print0 |
while IFS= read -r -d '' file; do
  base="${file##*/}"           # e.g., rulers.svg
  base="${base%.svg}"          # e.g., rulers
  name="bi-${base}-nav-menu"   # e.g., bi-rulers-nav-menu

  ./svg2css.sh "$file" "$name"
done
echo "Merged SVG icons into CSS classes."
