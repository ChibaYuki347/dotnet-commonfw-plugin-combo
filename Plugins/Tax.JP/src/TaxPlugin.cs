using System.Text.Json;
using Contoso.Plugin.Abstractions;

namespace Tax.JP;
public sealed class TaxPlugin : IPlugin
{
    public string Id => "Tax.JP";
    public string Version => "1.0.0";
    public Task<string> ExecuteAsync(string input, CancellationToken ct = default)
    {
        // 入力は JSON: { CountryCode, Amount }
        TaxRequest? req = null;
        try { req = JsonSerializer.Deserialize<TaxRequest>(input, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch { /* ignore and treat as passthrough */ }

        if (req is null)
            return Task.FromResult($"Tax.JP passthrough: {input}");

        // plugin.json から国コードと税率を取得（なければ JP/0.10 を既定）
        var (cfgCountry, cfgRate) = LoadConfigOrDefault();
        var apply = string.Equals(req.CountryCode, cfgCountry, StringComparison.OrdinalIgnoreCase);
        var rate = apply ? cfgRate : 0.00m;
        var tax = decimal.Round(req.Amount * rate, 2, MidpointRounding.AwayFromZero);
        var gross = req.Amount + tax;
        var result = new TaxResult(Net: req.Amount, Rate: rate, Tax: tax, Gross: gross, Applied: apply);
        var json = JsonSerializer.Serialize(result);
        return Task.FromResult(json);
    }

    private static (string Country, decimal Rate) LoadConfigOrDefault()
    {
        try
        {
            var asmDir = Path.GetDirectoryName(typeof(TaxPlugin).Assembly.Location)!;
            var jsonPath = Path.Combine(asmDir, "plugin.json");
            if (!File.Exists(jsonPath)) return ("JP", 0.10m);
            using var fs = File.OpenRead(jsonPath);
            using var doc = JsonDocument.Parse(fs);
            var root = doc.RootElement;
            var country = root.TryGetProperty("country", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() ?? "JP" : "JP";
            var rate = root.TryGetProperty("rate", out var r) && r.ValueKind is JsonValueKind.Number ? r.GetDecimal() : 0.10m;
            return (country, rate);
        }
        catch { return ("JP", 0.10m); }
    }
}
