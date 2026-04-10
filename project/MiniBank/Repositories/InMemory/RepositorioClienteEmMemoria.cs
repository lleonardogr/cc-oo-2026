using MiniBank.Models;
using MiniBank.Repositories.Contracts;

namespace MiniBank.Repositories.InMemory;

public class RepositorioClienteEmMemoria : IRepositorioCliente
{
    private readonly RepositorioEmMemoria<Cliente> repositorio = new();

    public void Salvar(Cliente cliente)
    {
        if (repositorio.BuscarPorId(cliente.Id) is null)
        {
            repositorio.Adicionar(cliente);
            return;
        }

        repositorio.Atualizar(cliente);
    }

    public Cliente? BuscarPorCpf(string cpf)
        => repositorio.BuscarPorId(cpf);

    public IEnumerable<Cliente> ListarTodos()
        => repositorio.ListarTodos();
}
