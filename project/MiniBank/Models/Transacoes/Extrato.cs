namespace MiniBank.Models.Transacoes;

public class Extrato
{
    private readonly List<Transacao> transacoes = [];

    public void Registrar(Transacao transacao)
    {
        transacoes.Add(transacao);
    }

    public IReadOnlyList<Transacao> Listar()
        => transacoes.AsReadOnly();

    public string Imprimir()
    {
        if (transacoes.Count == 0)
        {
            return "Nenhuma transacao registrada.";
        }

        return string.Join(Environment.NewLine, transacoes.Select(t => t.ToString()));
    }
}
