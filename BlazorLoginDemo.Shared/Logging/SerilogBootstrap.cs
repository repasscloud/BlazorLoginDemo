// BlazorLoginDemo.Shared/Logging/SerilogBootstrap.cs
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace BlazorLoginDemo.Shared.Logging;

public static class SerilogBootstrap
{
    /// <summary>
    /// Configure Serilog to write to PostgreSQL "public.logs" with useful enrichers.
    /// Call from Program.cs in BOTH apps before building the host.
    /// </summary>
    public static void UseSerilogWithPostgres(IConfiguration config, string appName)
    {
        var cs = config.GetConnectionString("LoggingDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:LoggingDb");

        // v2.3.0: set default VARCHAR length globally (affects SingleProperty columns, etc.)
        TableCreator.DefaultVarcharColumnsLength = 512;

        // v2.3.0 column writers live in Serilog.Sinks.PostgreSQL (no .ColumnWriters ns)
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            // message + template
            ["message"] = new RenderedMessageColumnWriter(NpgsqlDbType.Text),
            ["message_template"] = new MessageTemplateColumnWriter(NpgsqlDbType.Text),

            // level + timestamp
            ["level"] = new LevelColumnWriter(renderAsText: true, dbType: NpgsqlDbType.Varchar, columnLength: 64),
            ["timestamp"] = new TimestampColumnWriter(NpgsqlDbType.Timestamp), // v2.3.0 example uses Timestamp

            // exception + structured properties (JSONB)
            ["exception"] = new ExceptionColumnWriter(NpgsqlDbType.Text),
            ["properties"] = new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb),

            // request/user enrichments (UseSerilogRequestLogging will set these)
            ["request_path"] = new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
            ["request_id"] = new SinglePropertyColumnWriter("RequestId", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
            ["user_id"] = new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
            ["source_context"] = new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
            ["environment"] = new SinglePropertyColumnWriter("Environment", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
            ["application"] = new SinglePropertyColumnWriter("Application", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar),
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Console()
            // v2.3.0: pass columnWriters POSITIONALLY (named arg doesn't exist here)
            .WriteTo.PostgreSQL(
                cs,
                "logs",
                columnWriters,
                needAutoCreateTable: true,
                schemaName: "serilog")
            .CreateLogger();
    }
}
