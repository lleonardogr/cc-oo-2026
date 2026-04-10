using MiniBank.Models;
using MiniBank.Models.Contas;

namespace MiniBank.Factories;

public class FabricaContaCorrente : IFabricaConta<ContaCorrente>
{
    public ContaCorrente Criar(string numero, Cliente titular, decimal saldoInicial)
        => new(numero, titular, saldoInicial);
}
