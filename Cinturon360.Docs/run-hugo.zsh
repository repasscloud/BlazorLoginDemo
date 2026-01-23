#!/usr/bin/env zsh

set -euo pipefail

rm -rf ./public
hugo
npx pagefind --site public
hugo server
