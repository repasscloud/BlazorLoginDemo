using System.Text;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Security;
using Npgsql;

var exitCode = await CliApp.RunAsync(args);
return exitCode;

internal static class CliApp
{
    private const string ConnectionStringEnvVar = "CINTURON360_SYSTEM_ACCOUNT_CONNECTION_STRING";

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var invocation = CliInvocation.Parse(args);
            if (invocation.ShowHelp)
            {
                Console.WriteLine(UsageText.Text);
                return 0;
            }

            if (invocation.Command is null)
                invocation = PromptForInvocation();

            var connectionString = invocation.ConnectionString
                ?? Environment.GetEnvironmentVariable(ConnectionStringEnvVar);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"A connection string is required via --connection-string or {ConnectionStringEnvVar}.");

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var service = new SystemAccountAdminService(connection, new CinturonPasswordHasher());

            return invocation.Command switch
            {
                "create" => await ExecuteCreateAsync(service, invocation),
                "disable" => await ExecuteDisableAsync(service, invocation),
                "set-password" => await ExecuteSetPasswordAsync(service, invocation),
                "clear-password" => await ExecuteClearPasswordAsync(service, invocation),
                "status" => await ExecuteStatusAsync(service, invocation),
                _ => throw new InvalidOperationException($"Unknown command: {invocation.Command}")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> ExecuteCreateAsync(SystemAccountAdminService service, CliInvocation invocation)
    {
        var email = invocation.RequireOption("email");
        var firstName = invocation.RequireOption("first-name");
        var lastName = invocation.RequireOption("last-name");
        var plaintextPassword = invocation.GetOption("password");
        var isEmailVerified = invocation.HasFlag("email-verified");

        var status = await service.CreateOrUpdateSystemAccountAsync(email, firstName, lastName, plaintextPassword, isEmailVerified);
        WriteStatus(status);
        return 0;
    }

    private static async Task<int> ExecuteDisableAsync(SystemAccountAdminService service, CliInvocation invocation)
    {
        var email = invocation.RequireOption("email");
        var status = await service.DisableAsync(email);
        WriteStatus(status);
        return 0;
    }

    private static async Task<int> ExecuteSetPasswordAsync(SystemAccountAdminService service, CliInvocation invocation)
    {
        var email = invocation.RequireOption("email");
        var password = invocation.GetOption("password") ?? PromptSecret("Password: ");
        var status = await service.SetPasswordAsync(email, password);
        WriteStatus(status);
        return 0;
    }

    private static async Task<int> ExecuteClearPasswordAsync(SystemAccountAdminService service, CliInvocation invocation)
    {
        var email = invocation.RequireOption("email");
        var status = await service.ClearPasswordAsync(email);
        WriteStatus(status);
        return 0;
    }

    private static async Task<int> ExecuteStatusAsync(SystemAccountAdminService service, CliInvocation invocation)
    {
        var email = invocation.RequireOption("email");
        var status = await service.GetStatusAsync(email)
            ?? throw new InvalidOperationException($"No user found for {email}.");

        WriteStatus(status);
        return 0;
    }

    private static CliInvocation PromptForInvocation()
    {
        Console.WriteLine("Cinturon360 System Account Tool");
        Console.WriteLine("1. Create or update system account");
        Console.WriteLine("2. Disable account");
        Console.WriteLine("3. Set password");
        Console.WriteLine("4. Clear password");
        Console.WriteLine("5. Show status");
        Console.WriteLine("0. Exit");
        Console.Write("Choice: ");

        var choice = Console.ReadLine()?.Trim();
        if (choice is null or "0")
            return CliInvocation.Help();

        Console.Write("Connection string (leave blank to use env var): ");
        var connectionString = Console.ReadLine();
        Console.Write("Email: ");
        var email = ReadRequiredLine("Email is required.");

        return choice switch
        {
            "1" => BuildCreateInvocation(connectionString, email),
            "2" => CliInvocation.FromInteractive("disable", connectionString, new Dictionary<string, string?> { ["email"] = email }),
            "3" => BuildSetPasswordInvocation(connectionString, email),
            "4" => CliInvocation.FromInteractive("clear-password", connectionString, new Dictionary<string, string?> { ["email"] = email }),
            "5" => CliInvocation.FromInteractive("status", connectionString, new Dictionary<string, string?> { ["email"] = email }),
            _ => throw new InvalidOperationException("Unknown menu option.")
        };
    }

    private static CliInvocation BuildCreateInvocation(string? connectionString, string email)
    {
        Console.Write("First name: ");
        var firstName = ReadRequiredLine("First name is required.");
        Console.Write("Last name: ");
        var lastName = ReadRequiredLine("Last name is required.");
        Console.Write("Set password now? (y/N): ");
        var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
        string? password = null;
        if (answer is "y" or "yes")
            password = PromptSecret("Password: ");

        Console.Write("Mark email as verified? (y/N): ");
        var verified = Console.ReadLine()?.Trim().ToLowerInvariant() is "y" or "yes";

        return CliInvocation.FromInteractive(
            "create",
            connectionString,
            new Dictionary<string, string?>
            {
                ["email"] = email,
                ["first-name"] = firstName,
                ["last-name"] = lastName,
                ["password"] = password
            },
            verified ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "email-verified" } : null);
    }

    private static CliInvocation BuildSetPasswordInvocation(string? connectionString, string email)
    {
        var password = PromptSecret("Password: ");
        return CliInvocation.FromInteractive(
            "set-password",
            connectionString,
            new Dictionary<string, string?>
            {
                ["email"] = email,
                ["password"] = password
            });
    }

    private static string PromptSecret(string prompt)
    {
        Console.Write(prompt);
        var builder = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (builder.Length == 0)
                    continue;

                builder.Length--;
                continue;
            }

            if (!char.IsControl(key.KeyChar))
                builder.Append(key.KeyChar);
        }

        if (builder.Length == 0)
            throw new InvalidOperationException("Password is required.");

        return builder.ToString();
    }

    private static string ReadRequiredLine(string error)
    {
        var value = Console.ReadLine()?.Trim();
        return !string.IsNullOrWhiteSpace(value) ? value : throw new InvalidOperationException(error);
    }

    private static void WriteStatus(SystemAccountStatus status)
    {
        Console.WriteLine();
        Console.WriteLine($"User ID:            {status.UserId}");
        Console.WriteLine($"Email:              {status.Email}");
        Console.WriteLine($"User Category:      {status.UserCategory}");
        Console.WriteLine($"Platform Role:      {status.PlatformRole}");
        Console.WriteLine($"Active:             {status.IsActive}");
        Console.WriteLine($"Locked:             {status.IsLocked}");
        Console.WriteLine($"Suspended:          {status.IsSuspended}");
        Console.WriteLine($"Direct Login:       {status.AllowDirectLogin}");
        Console.WriteLine($"Has Password:       {status.HasPasswordHash}");
        Console.WriteLine($"Password Changed:   {status.PasswordChangedAt?.ToString("O") ?? "<null>"}");
        Console.WriteLine($"Lockout Until:      {status.LockoutUntil?.ToString("O") ?? "<null>"}");
        Console.WriteLine();
    }
}

internal sealed class SystemAccountAdminService(NpgsqlConnection connection, CinturonPasswordHasher passwordHasher)
{
    public async Task<SystemAccountStatus> CreateOrUpdateSystemAccountAsync(
        string email,
        string firstName,
        string lastName,
        string? plaintextPassword,
        bool isEmailVerified)
    {
        var normalizedEmail = NormalizeEmail(email);
        await using var tx = await connection.BeginTransactionAsync();

        var userId = await FindUserIdAsync(normalizedEmail, tx);
        if (userId is null)
        {
            userId = IdGenerator.NewUserId();
            await InsertUserAsync(userId, normalizedEmail, firstName, lastName, isEmailVerified, tx);
        }
        else
        {
            await UpdateUserAsSystemAccountAsync(userId, normalizedEmail, firstName, lastName, isEmailVerified, tx);
        }

        await EnsureSecurityRowAsync(userId, tx);

        if (!string.IsNullOrWhiteSpace(plaintextPassword))
            await SetPasswordInternalAsync(userId, plaintextPassword, tx);
        else
            await ResetDirectLoginPolicyAsync(userId, tx);

        await tx.CommitAsync();
        return await GetStatusByUserIdAsync(userId) ?? throw new InvalidOperationException("User status not found after create/update.");
    }

    public async Task<SystemAccountStatus> DisableAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var userId = await RequireUserIdAsync(normalizedEmail);
        await using var tx = await connection.BeginTransactionAsync();

        await using (var cmd = new NpgsqlCommand(
            """
            UPDATE users
            SET is_active = FALSE,
                is_locked = TRUE,
                is_suspended = TRUE,
                updated_at = now()
            WHERE id = @user_id;
            """, connection, tx))
        {
            cmd.Parameters.AddWithValue("user_id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand(
            """
            UPDATE user_security
            SET allow_direct_login = FALSE,
                lockout_until = now() + interval '100 years',
                updated_at = now()
            WHERE user_id = @user_id;
            """, connection, tx))
        {
            cmd.Parameters.AddWithValue("user_id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return await GetStatusByUserIdAsync(userId) ?? throw new InvalidOperationException("User status not found after disable.");
    }

    public async Task<SystemAccountStatus> SetPasswordAsync(string email, string plaintextPassword)
    {
        var normalizedEmail = NormalizeEmail(email);
        var userId = await RequireUserIdAsync(normalizedEmail);
        await using var tx = await connection.BeginTransactionAsync();
        await SetPasswordInternalAsync(userId, plaintextPassword, tx);
        await tx.CommitAsync();
        return await GetStatusByUserIdAsync(userId) ?? throw new InvalidOperationException("User status not found after password update.");
    }

    public async Task<SystemAccountStatus> ClearPasswordAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var userId = await RequireUserIdAsync(normalizedEmail);
        await using var tx = await connection.BeginTransactionAsync();

        await using (var cmd = new NpgsqlCommand(
            """
            UPDATE user_security
            SET password_hash = NULL,
                password_changed_at = NULL,
                password_expires_at = NULL,
                failed_login_attempts = 0,
                lockout_until = now() + interval '100 years',
                allow_direct_login = FALSE,
                updated_at = now()
            WHERE user_id = @user_id;
            """, connection, tx))
        {
            cmd.Parameters.AddWithValue("user_id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return await GetStatusByUserIdAsync(userId) ?? throw new InvalidOperationException("User status not found after clearing password.");
    }

    public async Task<SystemAccountStatus?> GetStatusAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        await using var cmd = new NpgsqlCommand(
            StatusSql,
            connection);
        cmd.Parameters.AddWithValue("email", normalizedEmail);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadStatus(reader) : null;
    }

    private async Task<SystemAccountStatus?> GetStatusByUserIdAsync(string userId)
    {
        await using var cmd = new NpgsqlCommand(
            StatusByUserIdSql,
            connection);
        cmd.Parameters.AddWithValue("user_id", userId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadStatus(reader) : null;
    }

    private async Task<string> RequireUserIdAsync(string normalizedEmail)
    {
        var userId = await FindUserIdAsync(normalizedEmail, transaction: null);
        return userId ?? throw new InvalidOperationException($"No user found for {normalizedEmail}.");
    }

    private async Task<string?> FindUserIdAsync(string normalizedEmail, NpgsqlTransaction? transaction)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT id FROM users WHERE email = @email AND is_deleted = FALSE LIMIT 1;",
            connection,
            transaction);
        cmd.Parameters.AddWithValue("email", normalizedEmail);
        var result = await cmd.ExecuteScalarAsync();
        return result as string;
    }

    private async Task InsertUserAsync(
        string userId,
        string normalizedEmail,
        string firstName,
        string lastName,
        bool isEmailVerified,
        NpgsqlTransaction tx)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO users (
                id, user_category, platform_role, home_org_id, email, first_name, last_name,
                is_email_verified, is_active, is_locked, is_suspended,
                language_code, time_zone, currency_code, avatar_storage_key, last_login_at,
                concurrency_stamp, created_at, updated_at, is_deleted, deleted_at, deleted_by_user_id
            ) VALUES (
                @id, 5, 1, NULL, @email, @first_name, @last_name,
                @is_email_verified, TRUE, FALSE, FALSE,
                'en', 'UTC', 'USD', NULL, NULL,
                @concurrency_stamp, now(), now(), FALSE, NULL, NULL
            );
            """, connection, tx);

        cmd.Parameters.AddWithValue("id", userId);
        cmd.Parameters.AddWithValue("email", normalizedEmail);
        cmd.Parameters.AddWithValue("first_name", firstName.Trim());
        cmd.Parameters.AddWithValue("last_name", lastName.Trim());
        cmd.Parameters.AddWithValue("is_email_verified", isEmailVerified);
        cmd.Parameters.AddWithValue("concurrency_stamp", Guid.NewGuid().ToString("N"));
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task UpdateUserAsSystemAccountAsync(
        string userId,
        string normalizedEmail,
        string firstName,
        string lastName,
        bool isEmailVerified,
        NpgsqlTransaction tx)
    {
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE users
            SET user_category = 5,
                platform_role = 1,
                home_org_id = NULL,
                email = @email,
                first_name = @first_name,
                last_name = @last_name,
                is_email_verified = @is_email_verified,
                is_active = TRUE,
                is_locked = FALSE,
                is_suspended = FALSE,
                is_deleted = FALSE,
                deleted_at = NULL,
                deleted_by_user_id = NULL,
                updated_at = now()
            WHERE id = @id;
            """, connection, tx);

        cmd.Parameters.AddWithValue("id", userId);
        cmd.Parameters.AddWithValue("email", normalizedEmail);
        cmd.Parameters.AddWithValue("first_name", firstName.Trim());
        cmd.Parameters.AddWithValue("last_name", lastName.Trim());
        cmd.Parameters.AddWithValue("is_email_verified", isEmailVerified);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task EnsureSecurityRowAsync(string userId, NpgsqlTransaction tx)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO user_security (
                id, user_id, password_hash, password_changed_at, password_expires_at,
                is_mfa_enabled, is_mfa_required, allow_direct_login,
                failed_login_attempts, lockout_until, created_at, updated_at
            ) VALUES (
                @id, @user_id, NULL, NULL, NULL,
                FALSE, FALSE, TRUE,
                0, NULL, now(), now()
            ) ON CONFLICT (user_id) DO NOTHING;
            """, connection, tx);

        cmd.Parameters.AddWithValue("id", IdGenerator.New("usec"));
        cmd.Parameters.AddWithValue("user_id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task SetPasswordInternalAsync(string userId, string plaintextPassword, NpgsqlTransaction tx)
    {
        var passwordHash = passwordHasher.Hash(plaintextPassword);
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE user_security
            SET password_hash = @password_hash,
                password_changed_at = now(),
                password_expires_at = NULL,
                failed_login_attempts = 0,
                lockout_until = NULL,
                allow_direct_login = TRUE,
                updated_at = now()
            WHERE user_id = @user_id;
            """, connection, tx);

        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("password_hash", passwordHash);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task ResetDirectLoginPolicyAsync(string userId, NpgsqlTransaction tx)
    {
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE user_security
            SET allow_direct_login = TRUE,
                failed_login_attempts = 0,
                lockout_until = NULL,
                updated_at = now()
            WHERE user_id = @user_id;
            """, connection, tx);

        cmd.Parameters.AddWithValue("user_id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToLowerInvariant();
    }

    private static SystemAccountStatus ReadStatus(NpgsqlDataReader reader)
        => new(
            UserId: reader.GetString(0),
            Email: reader.GetString(1),
            UserCategory: reader.GetInt32(2),
            PlatformRole: reader.IsDBNull(3) ? null : reader.GetInt32(3),
            IsActive: reader.GetBoolean(4),
            IsLocked: reader.GetBoolean(5),
            IsSuspended: reader.GetBoolean(6),
            AllowDirectLogin: !reader.IsDBNull(7) && reader.GetBoolean(7),
            LockoutUntil: reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
            HasPasswordHash: !reader.IsDBNull(9) && reader.GetBoolean(9),
            PasswordChangedAt: reader.IsDBNull(10) ? null : reader.GetFieldValue<DateTimeOffset>(10));

    private const string StatusSql =
        """
        SELECT
            u.id,
            u.email,
            u.user_category,
            u.platform_role,
            u.is_active,
            u.is_locked,
            u.is_suspended,
            s.allow_direct_login,
            s.lockout_until,
            (s.password_hash IS NOT NULL) AS has_password_hash,
            s.password_changed_at
        FROM users u
        LEFT JOIN user_security s ON s.user_id = u.id
        WHERE u.email = @email
          AND u.is_deleted = FALSE;
        """;

    private const string StatusByUserIdSql =
        """
        SELECT
            u.id,
            u.email,
            u.user_category,
            u.platform_role,
            u.is_active,
            u.is_locked,
            u.is_suspended,
            s.allow_direct_login,
            s.lockout_until,
            (s.password_hash IS NOT NULL) AS has_password_hash,
            s.password_changed_at
        FROM users u
        LEFT JOIN user_security s ON s.user_id = u.id
        WHERE u.id = @user_id
          AND u.is_deleted = FALSE;
        """;
}

internal sealed record SystemAccountStatus(
    string UserId,
    string Email,
    int UserCategory,
    int? PlatformRole,
    bool IsActive,
    bool IsLocked,
    bool IsSuspended,
    bool AllowDirectLogin,
    DateTimeOffset? LockoutUntil,
    bool HasPasswordHash,
    DateTimeOffset? PasswordChangedAt);

internal sealed class CliInvocation(
    string? command,
    string? connectionString,
    IReadOnlyDictionary<string, string?> options,
    IReadOnlySet<string> flags,
    bool showHelp = false)
{
    public string? Command { get; } = command;
    public string? ConnectionString { get; } = string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    public bool ShowHelp { get; } = showHelp;

    public static CliInvocation Parse(string[] args)
    {
        if (args.Length == 0)
            return new CliInvocation(null, null, new Dictionary<string, string?>(), new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        if (args[0] is "-h" or "--help" or "help")
            return Help();

        var command = args[0].Trim().ToLowerInvariant();
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
                throw new InvalidOperationException($"Unexpected argument: {arg}");

            var key = arg[2..];
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options[key] = args[++i];
                continue;
            }

            flags.Add(key);
        }

        options.TryGetValue("connection-string", out var connectionString);
        return new CliInvocation(command, connectionString, options, flags);
    }

    public static CliInvocation FromInteractive(
        string command,
        string? connectionString,
        IDictionary<string, string?> options,
        ISet<string>? flags = null)
        => new(command, connectionString, new Dictionary<string, string?>(options, StringComparer.OrdinalIgnoreCase), flags is null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : new HashSet<string>(flags, StringComparer.OrdinalIgnoreCase));

    public static CliInvocation Help()
        => new(null, null, new Dictionary<string, string?>(), new HashSet<string>(StringComparer.OrdinalIgnoreCase), true);

    public string RequireOption(string key)
        => GetOption(key) ?? throw new InvalidOperationException($"Missing required option --{key}.");

    public string? GetOption(string key)
        => options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    public bool HasFlag(string flag)
        => flags.Contains(flag);
}

internal static class UsageText
{
    public const string Text =
        """
        Cinturon360 System Account Tool

        Commands:
          create        --connection-string <value> --email <value> --first-name <value> --last-name <value> [--password <value>] [--email-verified]
          disable       --connection-string <value> --email <value>
          set-password  --connection-string <value> --email <value> [--password <value>]
          clear-password --connection-string <value> --email <value>
          status        --connection-string <value> --email <value>

        Connection string:
          Use --connection-string or set CINTURON360_SYSTEM_ACCOUNT_CONNECTION_STRING.

        Notes:
          - create/upsert forces UserCategory=Platform (5) and PlatformRole=Sudo (1)
          - set-password hashes plaintext with the same PBKDF2 SHA-512 format used by Cinturon
          - clear-password removes the hash and disallows direct login
          - disable does not clear the password hash; use clear-password if you want both

        Examples:
          cinturon-system-account create --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" --email sudo@example.com --first-name Sudo --last-name Admin --password ChangeMeNow! --email-verified
          cinturon-system-account disable --email sudo@example.com
          cinturon-system-account set-password --email sudo@example.com --password ChangeMeNow!
          cinturon-system-account clear-password --email sudo@example.com
          cinturon-system-account status --email sudo@example.com
        """;
}
