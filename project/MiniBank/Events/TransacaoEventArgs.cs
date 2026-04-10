using MiniBank.Contracts;
using MiniBank.Models.Transacoes;

namespace MiniBank.Events;

public sealed class TransacaoEventArgs : EventArgs
{
    public Transacao Transacao { get; }
    public IConta Conta { get; }

    public TransacaoEventArgs(Transacao transacao, IConta conta)
    {
        Transacao = transacao;
        Conta = conta;
    }
}
