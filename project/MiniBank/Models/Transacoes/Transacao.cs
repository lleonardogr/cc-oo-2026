namespace MiniBank.Models.Transacoes;

public record Transacao(
    decimal Valor,
    TipoTransacao Tipo,
    string Descricao,
    DateTime Data)
{
    public Transacao(decimal valor, TipoTransacao tipo, string descricao)
        : this(valor, tipo, descricao, DateTime.Now)
    {
    }

    public override string ToString()
        => $"{Data:dd/MM HH:mm} | {Tipo,-13} | {Valor,12:C} | {Descricao}";
}
