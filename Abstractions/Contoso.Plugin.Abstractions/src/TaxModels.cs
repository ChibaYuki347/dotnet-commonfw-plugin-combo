namespace Contoso.Plugin.Abstractions;

public sealed record TaxRequest(string CountryCode, decimal Amount);

public sealed record TaxResult(decimal Net, decimal Rate, decimal Tax, decimal Gross, bool Applied);
