# ECB Exchange Rate API Ingestion Notes

## Goal

Call the ECB exchange-rate API from a C# WebAPI or background job, read the compressed SDMX JSON response into memory, convert it into a clean application shape, and store it in the database.

## Curl call without saving to disk

Use this if you do **not** want to save the response to a file:

```bash
curl -L \
  "https://data-api.ecb.europa.eu/service/data/EXR/D..EUR.SP00.A?lastNObservations=1&detail=dataonly&format=jsondata"
```

Do **not** use this part if you do not want file output:

```bash
-z ./ecb-rates.json \
-o ./ecb-rates.json
```

Meaning:

```text
-z ./ecb-rates.json   = only fetch if remote data is newer than the local file
-o ./ecb-rates.json   = write the response body to this file
```

For a WebAPI or background job, call the URL with `HttpClient`, read the JSON into memory, parse it, then insert or update rows in PostgreSQL.

## ECB response shape

The ECB returns compressed SDMX JSON.

The top-level shape is:

```json
{
  "header": {
    "id": "string",
    "test": false,
    "prepared": "2026-04-29T17:36:03.651+02:00",
    "sender": {
      "id": "ECB"
    }
  },
  "dataSets": [
    {
      "action": "Replace",
      "validFrom": "2026-04-29T17:36:03.651+02:00",
      "series": {
        "0:1:0:0:0": {
          "observations": {
            "1": [
              1.6344
            ]
          }
        }
      }
    }
  ],
  "structure": {
    "name": "Exchange Rates",
    "dimensions": {
      "series": [
        {
          "id": "FREQ",
          "name": "Frequency",
          "values": [
            {
              "id": "D",
              "name": "Daily"
            }
          ]
        },
        {
          "id": "CURRENCY",
          "name": "Currency",
          "values": [
            {
              "id": "AUD",
              "name": "Australian dollar"
            }
          ]
        },
        {
          "id": "CURRENCY_DENOM",
          "name": "Currency denominator",
          "values": [
            {
              "id": "EUR",
              "name": "Euro"
            }
          ]
        },
        {
          "id": "EXR_TYPE",
          "name": "Exchange rate type",
          "values": [
            {
              "id": "SP00",
              "name": "Spot"
            }
          ]
        },
        {
          "id": "EXR_SUFFIX",
          "name": "Series variation - EXR context",
          "values": [
            {
              "id": "A",
              "name": "Average"
            }
          ]
        }
      ],
      "observation": [
        {
          "id": "TIME_PERIOD",
          "name": "Time period or range",
          "values": [
            {
              "id": "2026-04-29",
              "name": "2026-04-29",
              "start": "2026-04-29T00:00:00.000+02:00",
              "end": "2026-04-29T23:59:59.999+02:00"
            }
          ]
        }
      ]
    }
  }
}
```

## Top-level sections

```text
header      Metadata about the ECB response
dataSets    Actual compressed exchange-rate data
structure   Lookup tables needed to decode the compressed data
```

The actual rates are inside:

```text
dataSets[0].series
```

Example:

```json
"0:1:0:0:0": {
  "observations": {
    "1": [1.6344]
  }
}
```

## How to read the compressed series key

The series key:

```text
0:1:0:0:0
```

Maps to the `structure.dimensions.series` array:

```text
0 : 1 : 0 : 0 : 0
│   │   │   │   │
│   │   │   │   └─ EXR_SUFFIX index 0 = A
│   │   │   └───── EXR_TYPE index 0 = SP00
│   │   └───────── CURRENCY_DENOM index 0 = EUR
│   └───────────── CURRENCY index 1 = AUD
└───────────────── FREQ index 0 = D
```

The observation key:

```json
"1": [1.6344]
```

Maps to:

```text
structure.dimensions.observation[0].values[1]
```

So:

```text
TIME_PERIOD index 1 = 2026-04-29
VALUE              = 1.6344
```

Meaning:

```text
2026-04-29 | 1 EUR = 1.6344 AUD
```

## Clean application shape

Convert the ECB response into this clean in-memory shape:

```json
[
  {
    "source": "ECB",
    "baseCurrency": "EUR",
    "quoteCurrency": "AUD",
    "quoteCurrencyName": "Australian dollar",
    "rate": 1.6344,
    "rateDate": "2026-04-29",
    "preparedAt": "2026-04-29T17:36:03.651+02:00"
  },
  {
    "source": "ECB",
    "baseCurrency": "EUR",
    "quoteCurrency": "USD",
    "quoteCurrencyName": "US dollar",
    "rate": 1.1706,
    "rateDate": "2026-04-29",
    "preparedAt": "2026-04-29T17:36:03.651+02:00"
  }
]
```

This is the useful shape for application code and database storage.

## Recommended C# record

```csharp
public sealed record EcbExchangeRate(
    string Source,
    string BaseCurrency,
    string QuoteCurrency,
    string QuoteCurrencyName,
    decimal Rate,
    DateOnly RateDate,
    DateTimeOffset PreparedAt
);
```

## Recommended database entity

```csharp
public sealed class ExchangeRate
{
    public Guid Id { get; set; }

    public string Source { get; set; } = "ECB";

    public string BaseCurrency { get; set; } = "EUR";

    public string QuoteCurrency { get; set; } = "";

    public string QuoteCurrencyName { get; set; } = "";

    public decimal Rate { get; set; }

    public DateOnly RateDate { get; set; }

    public DateTimeOffset ProviderPreparedAt { get; set; }

    public DateTimeOffset ImportedAtUtc { get; set; }
}
```

## Important meaning of the rate

The ECB feed is quoted as:

```text
1 EUR = X quote currency
```

So this row:

```json
{
  "baseCurrency": "EUR",
  "quoteCurrency": "AUD",
  "rate": 1.6344
}
```

Means:

```text
1 EUR = 1.6344 AUD
```

It does **not** mean:

```text
1 AUD = 1.6344 EUR
```

## AUD-based usage

If your application is mostly AUD-based, still ingest the raw ECB data as EUR-based rates. Then derive AUD-based rates inside your application.

Example:

```csharp
decimal audPerEur = rates["AUD"];
decimal usdPerEur = rates["USD"];

decimal usdPerAud = usdPerEur / audPerEur;
```

Formula:

```text
QuoteCurrency per AUD = QuoteCurrency per EUR / AUD per EUR
```

Example:

```text
1 EUR = 1.6344 AUD
1 EUR = 1.1706 USD

1 AUD = 1.1706 / 1.6344 USD
1 AUD = 0.716218 USD
```

## C# WebAPI or background job call

```csharp
using System.Net.Http.Json;

var url = "https://data-api.ecb.europa.eu/service/data/EXR/D..EUR.SP00.A?lastNObservations=1&detail=dataonly&format=jsondata";

using var http = new HttpClient();

var response = await http.GetFromJsonAsync<EcbExchangeRateResponse>(url);

if (response is null)
    throw new InvalidOperationException("ECB returned no response.");

var rates = EcbExchangeRateParser.Parse(response);
```

## Claude instruction version

```text
Call the ECB URL without writing to disk. Parse the compressed SDMX JSON in memory. Decode dataSets[0].series using structure.dimensions.series and structure.dimensions.observation. Convert each observation into a clean ExchangeRate row with Source=ECB, BaseCurrency=EUR, QuoteCurrency, QuoteCurrencyName, Rate, RateDate, ProviderPreparedAt, and ImportedAtUtc. Store those rows in the database.
```

## Recommended stored database shape

```text
Source | BaseCurrency | QuoteCurrency | QuoteCurrencyName  | Rate    | RateDate
ECB    | EUR          | AUD           | Australian dollar  | 1.6344  | 2026-04-29
ECB    | EUR          | USD           | US dollar          | 1.1706  | 2026-04-29
ECB    | EUR          | GBP           | UK pound sterling  | 0.86643 | 2026-04-29
```

## Conversion formulas

EUR to foreign currency:

```csharp
decimal convertedAmount = amount * rate;
```

Foreign currency to EUR:

```csharp
decimal convertedAmount = amount / rate;
```

Foreign currency to another foreign currency:

```csharp
decimal convertedAmount = amount / fromRatePerEur * toRatePerEur;
```

Example conversion function:

```csharp
public static decimal ConvertCurrency(
    decimal amount,
    string fromCurrency,
    string toCurrency,
    IReadOnlyDictionary<string, decimal> ratesPerEur)
{
    fromCurrency = fromCurrency.ToUpperInvariant();
    toCurrency = toCurrency.ToUpperInvariant();

    if (fromCurrency == toCurrency)
        return amount;

    if (fromCurrency == "EUR")
        return amount * ratesPerEur[toCurrency];

    if (toCurrency == "EUR")
        return amount / ratesPerEur[fromCurrency];

    return amount / ratesPerEur[fromCurrency] * ratesPerEur[toCurrency];
}
```
