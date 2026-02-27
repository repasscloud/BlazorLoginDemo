#!/usr/bin/env bash
set -euo pipefail

SITE_DIR="${SITE_DIR:-/site}"
MODULE_PATH="${DOCS_MODULE_PATH:-example.com/cinturon360/docs}"

mkdir -p "${SITE_DIR}"

# Create a Hugo site skeleton (safe even if folder exists).
hugo new site "${SITE_DIR}" --force

cd "${SITE_DIR}"

# Initialize Hugo Modules (requires Go+Git in the container).
# Docsy is used as a Hugo Module. :contentReference[oaicite:1]{index=1}
hugo mod init "${MODULE_PATH}"

# Minimal Docsy config (module import).
cat > hugo.toml <<'TOML'
baseURL = "http://localhost:1313/"
title = "Cinturon360 Docs"
enableRobotsTXT = true

defaultContentLanguage = "en"
defaultContentLanguageInSubdir = true

[languages]
  [languages.en]
    languageName = "English"
    contentDir = "content/en"
    weight = 1

[module]
  [[module.imports]]
    path = "github.com/google/docsy"
TOML

# Node tooling for Docsy asset pipeline (simple baseline).
cat > package.json <<'JSON'
{
  "private": true,
  "devDependencies": {
    "autoprefixer": "^10.4.20",
    "postcss": "^8.4.49",
    "postcss-cli": "^11.0.0",
    "postcss-import": "^16.1.0"
  }
}
JSON

cat > postcss.config.js <<'JS'
module.exports = {
  plugins: [
    require('postcss-import'),
    require('autoprefixer')
  ]
};
JS

# Docsy hook file for your overrides (kept empty initially).
mkdir -p assets/scss
cat > assets/scss/_styles_project.scss <<'SCSS'
/* Project-level overrides go here. */
SCSS

# Minimal content so you get a visible homepage.
mkdir -p content/en
cat > content/en/_index.md <<'MD'
---
title: "Cinturon360 Documentation"
linkTitle: "Home"
menu:
  main:
    weight: 10
---

Welcome to the Cinturon360 docs.
MD

mkdir -p content/en/docs
cat > content/en/docs/_index.md <<'MD'
---
title: "Docs"
linkTitle: "Docs"
menu:
  main:
    weight: 20
---
MD

cat > content/en/docs/getting-started.md <<'MD'
---
title: "Getting started"
weight: 10
---

This is a placeholder page.
MD

# Pull module deps now (optional but avoids first-run surprises).
hugo mod get -u
