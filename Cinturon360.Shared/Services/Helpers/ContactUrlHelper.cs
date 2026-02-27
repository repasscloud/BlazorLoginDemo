using Microsoft.AspNetCore.WebUtilities;

namespace Cinturon360.Shared.Services.Helpers;

public static class ContactUrlHelper
{
    // Input can be "/support/contact?message=..&category=.."
    // or just "?message=..", or even "message=..&category=.."
    public static string NormalizeSupportContactUrl(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "/support/contact";

        string path = "/support/contact";
        string query = string.Empty;

        int qIdx = raw.IndexOf('?');
        if (qIdx >= 0)
        {
            path = qIdx == 0 ? "/support/contact" : raw[..qIdx].Trim();
            query = raw[(qIdx + 1)..];
        }
        else
        {
            // No '?', treat entire input as path or as bare query
            if (raw.Contains('='))
                query = raw.Trim();
            else
                path = string.IsNullOrWhiteSpace(raw) ? "/support/contact" : raw.Trim();
        }

        // Parse pairs without assuming prior URL-encoding
        var rawPairs = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(query))
        {
            foreach (var part in query.Split('&'))
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                var kv = part.Split('=', 2);
                var k = kv[0].Trim();
                if (string.IsNullOrEmpty(k)) continue;
                var v = kv.Length > 1 ? kv[1].Trim() : string.Empty; // leave raw; we will encode later
                rawPairs[k] = v; // last wins
            }
        }

        // Normalize allowed keys and aliases
        var norm = new Dictionary<string, string?>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var (kRaw, v) in rawPairs)
        {
            var k = kRaw.ToLowerInvariant();
            switch (k)
            {
                case "message":      norm["message"] = v; break;
                case "category":     norm["category"] = v; break;
                case "subcategory":  norm["subcategory"] = v; break;
                case "priority":     norm["priority"] = v; break;
                case "relatedurl":   norm["relatedurl"] = v; break;
                case "pageurl":      norm["pageUrl"] = v; break;
                case "description":  norm["description"] = v; break;
                default:             norm[kRaw] = v; break;
            }
        }

        // Ensure path defaults correctly
        if (string.IsNullOrWhiteSpace(path))
            path = "/support/contact";

        // Build encoded URL
        return norm.Count == 0 ? path : QueryHelpers.AddQueryString(path, norm);
    }
}
