#!/usr/bin/env zsh
# curl -X 'POST' \
#   'http://localhost:8090/v1/bookings/drafts/_yaSUdXHfZ976sdHq9sWh/flight-quotes/0' \
#   -H 'accept: application/json' \
#   -H 'X-Correlation-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6' \
#   -H 'X-Ava-ApiKey: ikUCnrCl29a5-QscggoA6YFIKsysjCGo4ELLJs_DIXjHSdS21rCrL80Wj1H7Siua' \
#   -H 'Content-Type: application/json' \
#   -d '{
#   "tid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
#   "uid": "string",
#   "org": "string"
# }'


resp="$(
  curl -sS \
    -X POST 'http://localhost:8090/v1/bookings/drafts/_yaSUdXHfZ976sdHq9sWh/flight-quotes/0' \
    -H 'accept: application/json' \
    -H 'X-Correlation-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6' \
    -H 'X-Ava-ApiKey: ikUCnrCl29a5-QscggoA6YFIKsysjCGo4ELLJs_DIXjHSdS21rCrL80Wj1H7Siua' \
    -H 'Content-Type: application/json' \
    -d '{
      "tid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "uid": "string",
      "org": "string"
    }'
)"

# byte size (Linux)
bytes=$(printf '%s' "$resp" | wc -c)

echo "Bytes: $bytes"
# echo "Body:"
# printf '%s\n' "$resp" | jq .
