using MiniBank.Repositories.Contracts;

namespace MiniBank.Repositories.InMemory;

public class RepositorioEmMemoria<T> : IRepositorio<T> where T : IIdentificavel
{
    protected readonly List<T> dados = [];

    public void Adicionar(T item)
    {
        if (dados.Any(d => d.Id == item.Id))
        {
            throw new InvalidOperationException($"Item '{item.Id}' ja existe.");
        }

        dados.Add(item);
    }

    public void Atualizar(T item)
    {
        var index = dados.FindIndex(d => d.Id == item.Id);
        if (index < 0)
        {
            throw new InvalidOperationException($"Item '{item.Id}' nao encontrado.");
        }

        dados[index] = item;
    }

    public bool Remover(string id)
    {
        var existente = BuscarPorId(id);
        return existente is not null && dados.Remove(existente);
    }

    public T? BuscarPorId(string id)
        => dados.FirstOrDefault(d => d.Id == id);

    public IEnumerable<T> ListarTodos()
        => dados.AsReadOnly();

    public IEnumerable<T> Buscar(Func<T, bool> filtro)
        => dados.Where(filtro).ToList();

    public int Contar()
        => dados.Count;
}
