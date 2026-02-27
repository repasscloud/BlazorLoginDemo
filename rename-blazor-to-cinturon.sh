#!/usr/bin/env bash
set -euo pipefail

OLD="Cinturon360"
NEW="Cinturon360"

# 1. pick sed
if command -v gsed >/dev/null 2>&1; then
  SED="gsed"
elif command -v sed >/dev/null 2>&1; then
  # assume GNU sed; if not, install gnu-sed on mac: brew install gnu-sed
  SED="sed"
else
  echo "sed/gsed not found" >&2
  exit 1
fi

echo "[1/3] renaming files and folders named with '$OLD' → '$NEW'..."

# rename paths first (deepest-first so children move before parents)
find . -depth -name "*${OLD}*" | while IFS= read -r path; do
  base=$(basename "$path")
  dir=$(dirname "$path")
  newbase=${base//$OLD/$NEW}
  if [[ "$base" != "$newbase" ]]; then
    mv -v -- "$path" "$dir/$newbase"
  fi
done

echo "[2/3] replacing contents in text-like files..."

# choose files to edit: common source/text; skip .git, bin, obj, images, etc.
find . \
  -type f \
  ! -path "*/.git/*" \
  ! -path "*/.vs/*" \
  ! -path "*/bin/*" \
  ! -path "*/obj/*" \
  ! -path "*/node_modules/*" \
  ! -path "*/.idea/*" \
  ! -path "*/.vscode/*" \
  \( -name "*.cs" -o -name "*.csproj" -o -name "*.props" -o -name "*.targets" \
     -o -name "*.sln" -o -name "*.razor" -o -name "*.cshtml" -o -name "*.json" \
     -o -name "*.yml" -o -name "*.yaml" -o -name "*.xml" -o -name "*.md" \
     -o -name "*.ts" -o -name "*.js" -o -name "*.css" -o -name "*.scss" \
     -o -name "*.html" -o -name "*.config" -o -name "*.sh" -o -name "*.ps1" \
     -o -name "*.txt" -o -name "*.dockerfile" -o -name "Dockerfile" -o -name "*.env" \
     -o -name "*.tf" -o -name "*.sql" \) \
  -print0 | while IFS= read -r -d '' file; do
    # only touch if it actually contains the old string
    if grep -q "$OLD" "$file"; then
      $SED -i "s/${OLD}/${NEW}/g" "$file"
      echo "updated: $file"
    fi
  done

echo "[3/3] done."
