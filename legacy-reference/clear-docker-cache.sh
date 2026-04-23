#!/usr/bin/env bash
set -euo pipefail

echo "🔴 Stopping all containers..."
ids="$(docker ps -aq)"
[ -n "${ids:-}" ] && docker stop $ids || true

echo "🧨 Removing all containers (and their anonymous volumes)..."
ids="$(docker ps -aq)"
[ -n "${ids:-}" ] && docker rm -fv $ids || true
docker container prune -f || true

echo "🗑  Removing dangling images (<none>:<none>) explicitly..."
dang="$(docker images -f 'dangling=true' -q || true)"
[ -n "${dang:-}" ] && docker rmi -f $dang || true

# Extra belt-and-braces: find any <none>:<none> using a stable format
more_dang="$(docker images --format '{{.Repository}} {{.Tag}} {{.ID}}' \
  | awk '$1=="<none>" && $2=="<none>" {print $3}')"
[ -n "${more_dang:-}" ] && docker rmi -f $more_dang || true

echo "🗑  Removing ALL images (tagged + untagged)..."
img="$(docker images -aq || true)"
[ -n "${img:-}" ] && docker rmi -f $img || true

echo "📦 Removing ALL volumes..."
vol="$(docker volume ls -q || true)"
[ -n "${vol:-}" ] && docker volume rm $vol || true
# or: docker volume prune -f

echo "🌐 Removing ALL non-default networks..."
# Default networks: bridge, host, none
net="$(docker network ls --format '{{.Name}}' | grep -Ev '^(bridge|host|none)$' || true)"
[ -n "${net:-}" ] && docker network rm $net || true
# or: docker network prune -f

echo "🧹 Clearing build cache (legacy builder and BuildKit/buildx)..."
docker builder prune -a -f
docker buildx prune -a -f

echo "🧼 Final sweep (dangling everything + volumes just in case)..."
docker system prune -a --volumes -f

echo "📏 Disk usage after cleanup:"
docker system df
