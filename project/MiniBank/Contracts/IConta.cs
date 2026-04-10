using MiniBank.Models;
using MiniBank.Repositories.Contracts;

namespace MiniBank.Contracts;

public interface IConta : IDebitavel, ICreditavel, IExibivel, IIdentificavel
{
    string Numero { get; }
    decimal Saldo { get; }
    Cliente Titular { get; }
    bool Ativa { get; }

    string ResumoRapido()
        => $"{Numero} | {Titular.Nome} | Saldo: {Saldo:C}";

    bool EstaPositiva()
        => Saldo >= 0;
}
