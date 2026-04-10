using MiniBank.Contracts;
using MiniBank.Repositories.Contracts;

namespace MiniBank.Repositories.InMemory;

public class RepositorioContaEmMemoria : IRepositorioConta
{
    private readonly List<IConta> contas = [];

    public void Salvar(IConta conta)
    {
        var existente = BuscarPorNumero(conta.Numero);
        if (existente is not null)
        {
            contas.Remove(existente);
        }

        contas.Add(conta);
    }

    public IConta? BuscarPorNumero(string numero)
        => contas.FirstOrDefault(c => c.Numero == numero);

    public IEnumerable<IConta> ListarTodas()
        => contas.AsReadOnly();

    public IEnumerable<IConta> BuscarPorCliente(string cpf)
        => contas.Where(c => c.Titular.Cpf == cpf).ToList();
}
