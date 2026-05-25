namespace SyntheticVitalsDemo.Api.Configuration;

public static class ConfigurationValues
{
    private const string LocalEnvFileName = "local.env";

    public static IReadOnlyDictionary<string, string?> LoadLocalEnv()
    {
        var path = FindLocalEnvPath();
        if (path is null)
        {
            return new Dictionary<string, string?>();
        }

        return File.ReadLines(path)
            .Select(ParseLine)
            .Where(x => x is not null)
            .ToDictionary(
                x => NormalizeKey(x!.Value.Key),
                x => x!.Value.Value,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeKey(string key) => key switch
    {
        "ConnectionString" => "ConnectionStrings:DefaultConnection",
        "ConnectionStrings_DefaultConnection" => "ConnectionStrings:DefaultConnection",
        _ => key.Replace("__", ":", StringComparison.Ordinal)
    };

    private static KeyValuePair<string, string?>? ParseLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#')) return null;

        var equalsIndex = trimmed.IndexOf('=');
        if (equalsIndex <= 0) return null;

        var key = trimmed[..equalsIndex].Trim();
        var value = trimmed[(equalsIndex + 1)..].Trim().Trim('"');
        return string.IsNullOrWhiteSpace(key) ? null : new KeyValuePair<string, string?>(key, value);
    }

    private static string? FindLocalEnvPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, LocalEnvFileName);
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }

        current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, LocalEnvFileName);
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }

        return null;
    }
}
