#!/bin/sh
pgrep -x crond >/dev/null 2>&1 || exit 2

[ -f /tmp/cron.heartbeat ] || exit 3
age=$(( $(date +%s) - $(cat /tmp/cron.heartbeat) ))
[ "$age" -le 600 ] || exit 4

if [ -f /tmp/cron.last ]; then
  ageh=$(( $(date +%s) - $(cat /tmp/cron.last) ))
  [ "$ageh" -le 4500 ] || exit 5
fi

exit 0
