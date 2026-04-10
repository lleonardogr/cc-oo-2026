using MiniBank.Contracts;
using MiniBank.Models.Contas;

namespace MiniBank.Analysis;

public static class AnalisadorConta
{
    public static string Classificar(IConta conta) => conta switch
    {
        ContaCorrente cc when cc.Saldo < 0 => $"Conta corrente {cc.Numero}: negativa ({cc.Saldo:C})",
        ContaCorrente cc => $"Conta corrente {cc.Numero}: {cc.Saldo:C}",
        ContaPoupanca cp when cp.Saldo >= 5_000m => $"Poupanca {cp.Numero}: saldo alto ({cp.Saldo:C})",
        ContaPoupanca cp => $"Poupanca {cp.Numero}: {cp.Saldo:C}",
        _ => $"Conta {conta.Numero}: {conta.Saldo:C}"
    };
}
