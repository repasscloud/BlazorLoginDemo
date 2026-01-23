
---
title: Environment & configuration
description: Environment variables used by Compose
weight: 60
---

This page documents the environment variables consumed by the Compose stack.

**Important**
- Values shown here are **placeholders only**
- Do **not** commit real secrets
- Actual values must be supplied via a local `.env` file

## General notes

- Environment variables are shared across multiple services
- Variables follow .NET double-underscore (`__`) binding conventions
- Some variables are consumed by infrastructure containers (Postgres, pgAdmin, pgweb)
- Others are consumed by application containers (API, webapp, migrator)

## UID / GID (Postgres filesystem mapping)

Used to ensure Postgres files are written with correct ownership on the host.

```env
UID=<host-user-id>
GID=<host-group-id>
```

## Postgres (cluster / admin)

```env
POSTGRES_HOST=db
POSTGRES_PORT=5432
POSTGRES_USER=<postgres-admin-user>
POSTGRES_PASSWORD=<postgres-admin-password>
POSTGRES_DB=postgres
```

## Application database (used by API & migrator)

```env
APP_DB_NAME=<app-database-name>
APP_DB_USER=<app-database-user>
APP_DB_PASSWORD=<app-database-password>
```

## Connection strings

Consumed by the API, webapp, and migrator containers.

```env
CONNECTIONSTRINGS__DEFAULTCONNECTION="Host=db;Port=5432;Database=<db>;Username=<user>;Password=<password>;Timeout=5;Command Timeout=5"
CONNECTIONSTRINGS__LOGGINGDB="Host=db;Port=5432;Database=<logging-db>;Username=<user>;Password=<password>;Timeout=5;Command Timeout=5"
```

## ASP.NET Core runtime

```env
ASPNETCORE_URLS="http://+:8080"
ASPNETCORE_ENVIRONMENT=<Development|Staging|Production>
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
```

## Logging

```env
Logging__LogLevel__Default=<Information|Debug|Warning>
Logging__LogLevel__Microsoft_AspNetCore=<Warning|Error>
Logging__LogLevel__Microsoft_EntityFrameworkCore_Database_Command=<Warning|Error>
```

## pgAdmin

Used by the pgAdmin service only.

```env
PGADMIN_DEFAULT_EMAIL=<admin-email>
PGADMIN_DEFAULT_PASSWORD=<admin-password>
```

## MailerSend

Used by the application for outbound email.

```env
MAILERSEND__APITOKEN=<mailersend-api-token>
MAILERSEND__FROMEMAIL=<from-email-address>
MAILERSEND__FROMNAME="<from-display-name>"
```

## Bootstrap / sudo credentials

Used for initial administrative access.

```env
ADMINEMAIL=<initial-admin-email>
ADMINPASSWORD=<initial-admin-password>
```

## Amadeus API

Used by the flight search / booking integration.

```env
AMADEUS__CLIENTID=<amadeus-client-id>
AMADEUS__CLIENTSECRET=<amadeus-client-secret>
AMADEUS__URL__APIENDPOINT=<oauth-endpoint-url>
AMADEUS__URL__FLIGHTOFFER=<flight-offers-endpoint-url>
```

## API key authentication

Inbound and outbound API key validation.

```env
INBOUNDAPIKEYAUTH__HEADERNAME=<header-name>
INBOUNDAPIKEYAUTH__ALLOWEDKEYS__0=<api-key-0>
INBOUNDAPIKEYAUTH__ALLOWEDKEYS__1=<api-key-1>
# ...
OUTBOUNDAPIKEYAUTH__HEADERNAME=<header-name>
OUTBOUNDAPIKEYAUTH__KEY=<outbound-api-key>
```

## Internal API addressing

```env
API__BASEADDRESS=http://api:8080
```

## pgweb

Database browser service.

```env
PGWEB_DATABASE_URL=postgres://<user>:<password>@db:5432/<db>?sslmode=disable
PGWEB_DATABASE_URL_ADMIN=postgres://<admin-user>:<admin-password>@db:5432/postgres?sslmode=disable
```

## Exchange rate API

```env
EXCHANGERATEAPI__BASEURL=<exchange-rate-api-base-url>
EXCHANGERATEAPI__APIKEY=<exchange-rate-api-key>
EXCHANGERATEAPI__DEFAULTBASECODE=<currency-code>
```

## Airline ingestion

Used by the airline ingestion process.

```env
AIRLINEINGESTION__SOURCEURL=<airline-source-url>
AIRLINEINGESTION__HTTPCLIENTNAME=<http-client-name>
```

## Security guidance

- Never commit `.env` files
- Rotate secrets regularly
- Use environment-specific values per developer / CI
- Treat API keys and passwords as **production-grade secrets**
