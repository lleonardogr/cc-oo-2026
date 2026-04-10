using MiniBank.Contracts;
using MiniBank.Models;

namespace MiniBank.Factories;

public interface IFabricaConta<out T> where T : IConta
{
    T Criar(string numero, Cliente titular, decimal saldoInicial);
}
