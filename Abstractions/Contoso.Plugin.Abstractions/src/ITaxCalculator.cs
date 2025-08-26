namespace Contoso.Plugin.Abstractions;

public interface ITaxCalculator
{
    TaxResult Calculate(TaxRequest request);
}
