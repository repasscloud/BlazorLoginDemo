#!/bin/zsh

find . -type d \( -name bin -o -name obj \) -prune -o -type f -name '*.cs' -print0 \
| xargs -0 sed -i 's/\bAva\.Shared\b/BlazorLoginDemo\.Shared/g'
