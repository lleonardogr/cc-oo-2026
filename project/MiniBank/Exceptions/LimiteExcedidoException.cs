namespace MiniBank.Exceptions;

public class LimiteExcedidoException : Exception
{
    public decimal LimiteDisponivel { get; }

    public LimiteExcedidoException(decimal limiteDisponivel)
        : base($"Limite disponivel insuficiente: {limiteDisponivel:C}.")
    {
        LimiteDisponivel = limiteDisponivel;
    }
}
