#!/usr/bin/env sh

curl -X 'POST' \
  'http://localhost:8090/v1/bookings/drafts/p03tq0fDG4UCEYTBHChOr/flight-quotes/0' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'X-Ava-ApiKey: Shq6_nO2alwM4rzXJaPeVVIxdDPoTP7bbjBqGjajoWysImi-3UiMZua8WdMv2cmY' \
  -d '{
  "tid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "uid": "string",
  "org": "string"
}'
