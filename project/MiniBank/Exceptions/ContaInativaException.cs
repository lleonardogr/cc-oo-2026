namespace MiniBank.Exceptions;

public class ContaInativaException : Exception
{
    public ContaInativaException(string numero)
        : base($"Conta {numero} esta inativa.")
    {
    }
}
