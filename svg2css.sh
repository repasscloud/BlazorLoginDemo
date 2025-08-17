#!/usr/bin/env bash
# Usage: ./svg2css.sh input.svg bi-person-gear-nav-menu

if [ $# -lt 2 ]; then
  echo "Usage: $0 input.svg css-class-name"
  exit 1
fi

SVG_FILE="$1"
CLASS_NAME="$2"
TARGET_CSS="$(dirname "$0")/BlazorLoginDemo.Web/Components/Layout/NavMenu.razor.css"

if [ ! -f "$SVG_FILE" ]; then
  echo "Error: File '$SVG_FILE' not found."
  exit 1
fi

# Read and clean the SVG (single line, replace double quotes with single)
SVG=$(cat "$SVG_FILE" | tr -d '\n' | sed "s/\"/'/g")

# Force fill to white (replace currentColor if present)
SVG=$(echo "$SVG" | sed "s/fill='currentColor'/fill='white'/g")

# URL-encode reserved characters
ENCODED=$(echo "$SVG" | sed -e 's/#/%23/g' -e 's/</%3C/g' -e 's/>/%3E/g')

# Final CSS rule
CSS=".$CLASS_NAME {
    background-image: url(\"data:image/svg+xml,$ENCODED\");
}
"

# Always print to stdout
echo "$CSS"

# If NavMenu.razor.css exists, append
if [ -f "$TARGET_CSS" ]; then
  echo "" >> "$TARGET_CSS"
  echo "$CSS" >> "$TARGET_CSS"
  echo "✅ Appended to $TARGET_CSS"
fi
