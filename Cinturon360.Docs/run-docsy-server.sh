#!/usr/bin/env zsh

set -euo pipefail

hugo_docsy() {
  docker run --rm -it \
    -p 1313:1313 \
    -v "$PWD":/src \
    -w /src \
    floryn90/hugo:0.152.2-ext-ubuntu-onbuild "$@"
}

npm_docsy() {
  docker run --rm -it \
    -v "$PWD":/src \
    -w /src \
    --entrypoint npm \
    floryn90/hugo:0.152.2-ext-ubuntu-onbuild "$@"
}

npm_docsy install
hugo_docsy server -D --bind 0.0.0.0 --baseURL http://localhost:1313
