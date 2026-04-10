namespace MiniBank.Exceptions;

public class SaldoInsuficienteException : Exception
{
    public decimal SaldoAtual { get; }
    public decimal ValorSolicitado { get; }

    public SaldoInsuficienteException(decimal saldoAtual, decimal valorSolicitado)
        : base($"Saldo {saldoAtual:C} insuficiente para operacao de {valorSolicitado:C}.")
    {
        SaldoAtual = saldoAtual;
        ValorSolicitado = valorSolicitado;
    }
}
