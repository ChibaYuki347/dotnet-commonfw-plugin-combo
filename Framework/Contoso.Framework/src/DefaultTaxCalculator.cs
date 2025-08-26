using Contoso.Plugin.Abstractions;

namespace Contoso.Framework;

public sealed class DefaultTaxCalculator : ITaxCalculator
{
    // 既定は 0%（プラグインが上書きする前提）
    public TaxResult Calculate(TaxRequest request)
    {
        var rate = 0.00m;
        var tax = decimal.Round(request.Amount * rate, 2, MidpointRounding.AwayFromZero);
        return new TaxResult(Net: request.Amount, Rate: rate, Tax: tax, Gross: request.Amount + tax, Applied: false);
    }
}
