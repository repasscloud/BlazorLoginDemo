# Cinturon360 System Account Tool

This folder now contains a dedicated C# admin CLI for manual system-account management.

It is intentionally **not** run as part of Docker Compose startup.

## What It Does

The tool manages a highest-privilege platform account directly against PostgreSQL using a connection string.

Supported operations:

- Create or upsert a system account
- Disable an account
- Set a plaintext password using the same hashing format as Cinturon
- Clear the password hash
- Show current account status

Create/upsert forces:

- `user_category = 5` (`Platform`)
- `platform_role = 1` (`Sudo`)

The hashing logic is code-based, not external SQL, and is shared with the application via [src/Cinturon360.Common/Security/CinturonPasswordHasher.cs](src/Cinturon360.Common/Security/CinturonPasswordHasher.cs).

## Files

- `Cinturon360.SystemAccountTool.csproj`
- `Program.cs`

## Connection String

Use either:

- `--connection-string "..."`
- `CINTURON360_SYSTEM_ACCOUNT_CONNECTION_STRING`

Example for local Docker dev:

```bash
Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password
```

That works because the dev compose file exposes Postgres on `5432`.

## Usage

Run from source:

```bash
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj -- \
  create \
  --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" \
  --email sudo@example.com \
  --first-name Sudo \
  --last-name Admin \
  --password ChangeMeNow! \
  --email-verified
```

Other commands:

```bash
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj -- disable --email sudo@example.com
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj -- set-password --email sudo@example.com --password ChangeMeNow!
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj -- clear-password --email sudo@example.com
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj -- status --email sudo@example.com
```

If `--connection-string` is omitted, the tool reads `CINTURON360_SYSTEM_ACCOUNT_CONNECTION_STRING`.

## Interactive Mode

Run with no command to use the menu:

```bash
dotnet run --project tools/db/system-account/Cinturon360.SystemAccountTool.csproj
```

## Native AOT Single Binary Publish

The project is configured for native AOT and self-contained single-binary publishing.

Examples:

```bash
dotnet publish tools/db/system-account/Cinturon360.SystemAccountTool.csproj -c Release -r linux-x64
dotnet publish tools/db/system-account/Cinturon360.SystemAccountTool.csproj -c Release -r osx-arm64
dotnet publish tools/db/system-account/Cinturon360.SystemAccountTool.csproj -c Release -r win-x64
```

On macOS, the project includes linker search paths for standard Homebrew installs of `openssl@3` and `brotli` on both Apple Silicon and Intel.

Published output will be under:

```bash
tools/db/system-account/bin/Release/net10.0/<rid>/publish/
```

## Support-Team Deployment Model

For support usage in production:

1. Publish the native binary for the target OS.
2. Provide the support operator with the binary only.
3. Pass the production connection string securely via environment variable or secret store.
4. Run only the explicit command needed.

## Notes

- `disable` does not clear the password hash.
- `clear-password` removes the hash and blocks direct login.
- `set-password` accepts plaintext and hashes it with the same PBKDF2 SHA-512 format used by Cinturon.
