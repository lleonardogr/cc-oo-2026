namespace MiniBank.Strategies;

public class TaxaContaPremium : ICalculadoraTaxa
{
    public decimal Calcular(decimal valor) => valor > 10_000m ? 0m : valor * 0.01m;
}
