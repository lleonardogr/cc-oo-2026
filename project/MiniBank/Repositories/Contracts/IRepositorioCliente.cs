using MiniBank.Models;

namespace MiniBank.Repositories.Contracts;

public interface IRepositorioCliente
{
    void Salvar(Cliente cliente);
    Cliente? BuscarPorCpf(string cpf);
    IEnumerable<Cliente> ListarTodos();
}
