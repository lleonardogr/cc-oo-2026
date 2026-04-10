using MiniBank.Contracts;

namespace MiniBank.Repositories.Contracts;

public interface IRepositorioConta
{
    void Salvar(IConta conta);
    IConta? BuscarPorNumero(string numero);
    IEnumerable<IConta> ListarTodas();
    IEnumerable<IConta> BuscarPorCliente(string cpf);
}
