using MiniBank.Contracts;
using MiniBank.Events;
using MiniBank.Exceptions;
using MiniBank.Models.Transacoes;

namespace MiniBank.Models.Contas;

public abstract class ContaBase : IConta
{
    public string Id => Numero;
    public string Numero { get; }
    public decimal Saldo { get; protected set; }
    public Cliente Titular { get; }
    public bool Ativa { get; private set; } = true;
    public Extrato Extrato { get; } = new();

    public event EventHandler<TransacaoEventArgs>? TransacaoRealizada;

    protected ContaBase(string numero, Cliente titular, decimal saldoInicial = 0m)
    {
        Numero = !string.IsNullOrWhiteSpace(numero) ? numero : throw new ArgumentException("Numero obrigatorio.", nameof(numero));
        Titular = titular ?? throw new ArgumentNullException(nameof(titular));

        if (saldoInicial < 0)
        {
            throw new ArgumentException("Saldo inicial nao pode ser negativo.", nameof(saldoInicial));
        }

        Saldo = saldoInicial;

        if (saldoInicial > 0)
        {
            Extrato.Registrar(new Transacao(saldoInicial, TipoTransacao.Deposito, "Saldo inicial"));
        }
    }

    public void Depositar(decimal valor)
    {
        GarantirContaAtiva();

        if (valor <= 0)
        {
            throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        }

        Saldo += valor;
        NotificarTransacao(new Transacao(valor, TipoTransacao.Deposito, "Deposito"));
    }

    public abstract bool Sacar(decimal valor);

    public virtual string ExibirExtrato()
    {
        return
            $"[{GetType().Name}] Conta {Numero} | Titular: {Titular.Nome} | Saldo: {Saldo:C}{Environment.NewLine}" +
            Extrato.Imprimir();
    }

    public void Desativar()
    {
        Ativa = false;
    }

    protected void GarantirContaAtiva()
    {
        if (!Ativa)
        {
            throw new ContaInativaException(Numero);
        }
    }

    protected void NotificarTransacao(Transacao transacao)
    {
        Extrato.Registrar(transacao);
        TransacaoRealizada?.Invoke(this, new TransacaoEventArgs(transacao, this));
    }
}
