#!/bin/zsh

find . -type d \( -name bin -o -name obj \) -prune -o -type f -name '*.cs' -print0 \
| xargs -0 gsed -i 's/\bBlazorLoginDemo\b/Cinturon360/g'
