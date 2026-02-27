#!/bin/bash

curl -i -X POST \
  'http://localhost:8090/v1/bookings/drafts/8OFkxoNFHwzwJu_InurCu/flight-searches' \
  -H 'Accept: */*' \
  -H 'Content-Type: application/json' \
  -H 'X-Ava-ApiKey: Shq6_nO2alwM4rzXJaPeVVIxdDPoTP7bbjBqGjajoWysImi-3UiMZua8WdMv2cmY' \
  -d '{
    "id": "8OFkxoNFHwzwJu_InurCu",
    "tripType": "OneWay",
    "originIataCode": "SYD",
    "destinationIataCode": "LHR",
    "departureDate": "2026-02-05",
    "returnDate": null,
    "departEarliestTime": "",
    "departLatestTime": "",
    "returnEarliestTime": "",
    "returnLatestTime": "",
    "cabinClass": "Economy",
    "maxCabinClass": "Economy",
    "selectedAirlines": ["QF"],
    "alliances": [],
    "tid": "ef226dab-264e-41f1-9d34-84c2633d96fa",
    "uid": "1074291e-f876-4246-84cc-be4b9f2a8c3b",
    "org": null
  }'


curl -i -X POST \
  'http://localhost:8090/v1/bookings/drafts/8OFkxoNFHwzwJu_InurCu/flight-quotes' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'X-Ava-ApiKey: Shq6_nO2alwM4rzXJaPeVVIxdDPoTP7bbjBqGjajoWysImi-3UiMZua8WdMv2cmY' \
  -d '{
  "tid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "uid": "string",
  "org": "string"
}'