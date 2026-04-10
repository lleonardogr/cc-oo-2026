using MiniBank.Contracts;

namespace MiniBank.DTOs;

public record ContaResumo(string Numero, string NomeTitular, decimal Saldo, string Tipo)
{
    public static ContaResumo Converter(IConta conta)
        => new(conta.Numero, conta.Titular.Nome, conta.Saldo, conta.GetType().Name);
}
