namespace MiniBank.Strategies;

public class TaxaContaCorrente : ICalculadoraTaxa
{
    public decimal Calcular(decimal valor) => valor * 0.02m;
}
