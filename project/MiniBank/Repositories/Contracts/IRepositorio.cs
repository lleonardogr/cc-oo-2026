namespace MiniBank.Repositories.Contracts;

public interface IRepositorio<T> where T : IIdentificavel
{
    void Adicionar(T item);
    void Atualizar(T item);
    bool Remover(string id);
    T? BuscarPorId(string id);
    IEnumerable<T> ListarTodos();
    IEnumerable<T> Buscar(Func<T, bool> filtro);
    int Contar();
}
