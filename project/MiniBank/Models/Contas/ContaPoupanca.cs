using MiniBank.Exceptions;
using MiniBank.Models.Transacoes;

namespace MiniBank.Models.Contas;

public class ContaPoupanca : ContaBase
{
    public decimal TaxaRendimento { get; }

    public ContaPoupanca(string numero, Cliente titular, decimal saldoInicial = 0m, decimal taxaRendimento = 0.005m)
        : base(numero, titular, saldoInicial)
    {
        TaxaRendimento = taxaRendimento;
    }

    public override bool Sacar(decimal valor)
    {
        GarantirContaAtiva();

        if (valor <= 0)
        {
            throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        }

        if (valor > Saldo)
        {
            throw new SaldoInsuficienteException(Saldo, valor);
        }

        Saldo -= valor;
        NotificarTransacao(new Transacao(valor, TipoTransacao.Saque, "Saque poupanca"));
        return true;
    }

    public void AplicarRendimento()
    {
        GarantirContaAtiva();

        var rendimento = Saldo * TaxaRendimento;
        Saldo += rendimento;
        NotificarTransacao(new Transacao(rendimento, TipoTransacao.Rendimento, "Aplicacao de rendimento"));
    }
}
