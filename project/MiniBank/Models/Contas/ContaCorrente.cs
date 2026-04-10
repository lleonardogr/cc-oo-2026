using MiniBank.Exceptions;
using MiniBank.Models.Transacoes;

namespace MiniBank.Models.Contas;

public class ContaCorrente : ContaBase
{
    public decimal LimiteChequeEspecial { get; }

    public ContaCorrente(string numero, Cliente titular, decimal saldoInicial = 0m, decimal limiteChequeEspecial = 500m)
        : base(numero, titular, saldoInicial)
    {
        LimiteChequeEspecial = limiteChequeEspecial;
    }

    public override bool Sacar(decimal valor)
    {
        GarantirContaAtiva();

        if (valor <= 0)
        {
            throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        }

        var disponivel = Saldo + LimiteChequeEspecial;
        if (valor > disponivel)
        {
            throw new SaldoInsuficienteException(disponivel, valor);
        }

        if (valor > Saldo && LimiteChequeEspecial <= 0)
        {
            throw new LimiteExcedidoException(LimiteChequeEspecial);
        }

        Saldo -= valor;
        NotificarTransacao(new Transacao(valor, TipoTransacao.Saque, "Saque conta corrente"));
        return true;
    }

    public override string ExibirExtrato()
        => base.ExibirExtrato() + $"{Environment.NewLine}Limite: {LimiteChequeEspecial:C}";
}
