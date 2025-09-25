using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

// Build config: appsettings.json first, then environment vars override
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)              // look in bin/… where the file is copied
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

// Helper: prefer simple env vars if present, else nested config keys
string? Get(string primaryEnvOrKey, string? fallbackKey = null)
    => config[primaryEnvOrKey] ?? (fallbackKey is null ? null : config[fallbackKey]);

var apiKey   = Get("MAILERSEND_API_KEY", "MailerSend:ApiKey");
var from     = Get("MAILERSEND_FROM",    "MailerSend:FromEmail");
var to       = Get("MAILERSEND_TO",      "MailerSend:ToEmail");
var fromName = Get("MAILERSEND_FROM_NAME","MailerSend:FromName") ?? "MailerSend Smoke";

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
{
    Console.Error.WriteLine("Missing config. Provide either:");
    Console.Error.WriteLine("- Env vars: MAILERSEND_API_KEY, MAILERSEND_FROM, MAILERSEND_TO");
    Console.Error.WriteLine("- Or appsettings.json: MailerSend:{ApiKey,FromEmail,ToEmail}");
    return 1;
}

using var http = new HttpClient();
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var html = $"<p>This is a MailerSend test sent at {DateTimeOffset.UtcNow:u}</p>";
var text = Regex.Replace(Regex.Replace(html, "<[^>]+>", " "), @"\s+", " ").Trim();

var payload = new
{
    from = new { email = from, name = fromName },
    to   = new[] { new { email = to } },
    subject = "MailerSend smoke test",
    text,
    html
};

var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.mailersend.com/v1/email")
{
    Content = new StringContent(json, Encoding.UTF8, "application/json")
};

Console.WriteLine("Sending…");
var resp = await http.SendAsync(req);
var body = await resp.Content.ReadAsStringAsync();

Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
Console.WriteLine("Response:\n" + body);

return resp.IsSuccessStatusCode ? 0 : 2;
