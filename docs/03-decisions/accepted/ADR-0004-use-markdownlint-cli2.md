# ADR-0004: Use markdownlint-cli2 for Markdown Linting

**Status:** Accepted  
**Date:** 2026-05-10  
**Deciders:** Cinturon360 engineering team

## Context

The Cinturon360 repository contains a significant body of Markdown documentation: planning documents, runbooks, architecture references, ADRs, and a QUESTIONS.md file over 2 200 lines. As documentation grows (especially after the planned migration to the Obsidian `docs/` structure), consistent Markdown formatting prevents diff noise and enforces authoring rules.

`claude-code-v5-to-v5-1-migration-discovery-prompt.md` identified `markdownlint-cli2` as the chosen linter. This ADR codifies that choice.

`markdownlint-cli2` is the successor to `markdownlint-cli`; it is faster (runs in parallel), natively supports glob patterns without a shell wrapper, and is maintained by the same author as the VS Code markdownlint extension.

## Decision

`markdownlint-cli2` is the standard Markdown linter for this repository.

Configuration file: `.markdownlint.yaml` (or `.markdownlint-cli2.yaml`) at the repository root.

Intended integration points:
- Pre-commit hook (to be added to `.git/hooks/pre-commit` or via a hook manager such as `lefthook`)
- CI step (once `.github/workflows/` is created)
- VS Code `davidanson.vscode-markdownlint` extension for inline feedback

## Consequences

**Positive**
- Uniform heading hierarchies, link formats, and list styles across all Markdown files.
- Fast parallel execution suitable for large `docs/` trees.
- Config file can suppress specific rules per directory (e.g., `docs/99-archive/` may relax line-length rules).

**Negative / Trade-offs**
- Requires Node.js (already present in the Web project for Tailwind; `package.json` should install `markdownlint-cli2` as a dev dependency or a separate root-level `package.json` created).
- Existing Markdown files at the repository root will likely have linting violations that need a one-time cleanup pass.

**Required follow-up**
- Add `markdownlint-cli2` to a root-level `package.json` (or the existing Web `package.json`) as a devDependency.
- Author `.markdownlint.yaml` with agreed rule set.
- Run a one-time lint + fix pass over existing Markdown before enabling CI gate.
- Add a CI step once `.github/workflows/` is scaffolded.
