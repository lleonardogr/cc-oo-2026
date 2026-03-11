# Aula 10 - Generics em Profundidade

## Teoria

Generics criam componentes reutilizaveis com seguranca de tipo. `<T>` e placeholder substituido por tipo concreto no uso. Disponivel desde C# 2, presente em todo o ecossistema .NET (`List<T>`, `Dictionary<TKey, TValue>`, `Task<T>`).

### Por que nao usar `object`?

Sem generics, usariamos `object` — causando casting manual, sem seguranca de tipo em compilacao e boxing/unboxing para value types.

### Constraints

```csharp
where T : class         // tipo de referencia
where T : struct        // tipo de valor
where T : new()         // construtor sem parametros
where T : NomeClasse    // heranca
where T : IInterface    // implementa interface
```

### Covariancia e Contravariancia

- `out T` (covariancia): permite usar tipo mais derivado. T so em saida.
- `in T` (contravariancia): permite usar tipo mais geral. T so em entrada.

---

## 🏦 Hands-on: App Bancario — Repositorio generico unificado

Na Aula 6 criamos um `Repositorio<T>` basico e na Aula 9 criamos repositorios especificos. Agora vamos unificar tudo num repositorio generico robusto com constraints, metodos de busca e covariancia.

### Passo 1: Interface generica com constraints

```csharp
// === MiniBank v0.9 — Generics avancados ===

public interface IIdentificavel
{
    string Id { get; }
}

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
```

### Passo 2: Implementacao generica

```csharp
public class RepositorioEmMemoria<T> : IRepositorio<T> where T : IIdentificavel
{
    protected readonly List<T> dados = new();

    public void Adicionar(T item)
    {
        if (dados.Any(d => d.Id == item.Id))
            throw new InvalidOperationException($"Item '{item.Id}' ja existe.");
        dados.Add(item);
    }

    public void Atualizar(T item)
    {
        var index = dados.FindIndex(d => d.Id == item.Id);
        if (index < 0) throw new InvalidOperationException($"Item '{item.Id}' nao encontrado.");
        dados[index] = item;
    }

    public bool Remover(string id)
    {
        var item = BuscarPorId(id);
        return item != null && dados.Remove(item);
    }

    public T? BuscarPorId(string id) => dados.FirstOrDefault(d => d.Id == id);
    public IEnumerable<T> ListarTodos() => dados.AsReadOnly();
    public IEnumerable<T> Buscar(Func<T, bool> filtro) => dados.Where(filtro);
    public int Contar() => dados.Count;
}
```

### Passo 3: Fazendo as entidades implementarem IIdentificavel

```csharp
public class Cliente : IIdentificavel
{
    public string Id => Cpf;
    // ... Nome, Cpf, Email, etc.
}

public abstract class ContaBase : IConta, IIdentificavel
{
    public string Id => Numero;
    // ... Numero, Saldo, Titular, etc.
}
```

### Passo 4: Repositorio especializado via heranca generica

Se precisarmos de metodos especificos, herdamos do generico:

```csharp
public class RepositorioConta : RepositorioEmMemoria<ContaBase>, IRepositorioConta
{
    // Metodo especifico para contas
    public IEnumerable<ContaBase> BuscarPorCliente(string cpf)
        => Buscar(c => c.Titular.Cpf == cpf);

    public IEnumerable<ContaBase> ContasComSaldoNegativo()
        => Buscar(c => c.Saldo < 0);

    // Adaptacao para a interface IRepositorioConta
    public void Salvar(IConta conta)
    {
        if (conta is ContaBase cb)
        {
            if (BuscarPorId(cb.Id) != null)
                Atualizar(cb);
            else
                Adicionar(cb);
        }
    }

    IConta? IRepositorioConta.BuscarPorNumero(string numero) => BuscarPorId(numero);
    IEnumerable<IConta> IRepositorioConta.ListarTodas() => ListarTodos();
    IEnumerable<IConta> IRepositorioConta.BuscarPorCliente(string cpf) => BuscarPorCliente(cpf);
}
```

### Passo 5: Metodos genericos utilitarios

```csharp
public static class ConsultaUtils
{
    // Metodo generico para encontrar o maior por uma propriedade
    public static T? MaiorPor<T, TKey>(IEnumerable<T> colecao, Func<T, TKey> seletor)
        where TKey : IComparable<TKey>
    {
        return colecao.OrderByDescending(seletor).FirstOrDefault();
    }

    // Metodo generico para agrupar
    public static Dictionary<TKey, List<T>> AgruparPor<T, TKey>(
        IEnumerable<T> colecao, Func<T, TKey> seletor) where TKey : notnull
    {
        var resultado = new Dictionary<TKey, List<T>>();
        foreach (var item in colecao)
        {
            var chave = seletor(item);
            if (!resultado.ContainsKey(chave))
                resultado[chave] = new List<T>();
            resultado[chave].Add(item);
        }
        return resultado;
    }
}
```

### Passo 6: Covariancia — produtor de contas

```csharp
public interface IFabricaConta<out T> where T : IConta
{
    T Criar(string numero, Cliente titular, decimal saldoInicial);
}

public class FabricaContaCorrente : IFabricaConta<ContaCorrente>
{
    public ContaCorrente Criar(string numero, Cliente titular, decimal saldoInicial)
        => new ContaCorrente(numero, titular, saldoInicial);
}

// Covariancia: IFabricaConta<ContaCorrente> pode ser atribuido a IFabricaConta<IConta>
IFabricaConta<IConta> fabrica = new FabricaContaCorrente(); // OK!
IConta novaConta = fabrica.Criar("CC-999", ana, 1000m);
```

### Testando tudo

```csharp
var repoClientes = new RepositorioEmMemoria<Cliente>();
var repoContas = new RepositorioConta();

var ana = new Cliente("Ana Silva", "123.456.789-00", "ana@email.com");
var joao = new Cliente("Joao Santos", "987.654.321-00", "joao@email.com");

repoClientes.Adicionar(ana);
repoClientes.Adicionar(joao);

var cc1 = new ContaCorrente("CC-001", ana, 5000m);
var cc2 = new ContaCorrente("CC-002", joao, 1000m);
var cp1 = new ContaPoupanca("CP-001", ana, 8000m);

repoContas.Adicionar(cc1);
repoContas.Adicionar(cc2);
repoContas.Adicionar(cp1);

// Consultas genericas
Console.WriteLine($"Total de clientes: {repoClientes.Contar()}");
Console.WriteLine($"Total de contas: {repoContas.Contar()}");

var contasAna = repoContas.BuscarPorCliente("123.456.789-00");
foreach (var c in contasAna)
    Console.WriteLine(c.ExibirExtrato());

// Maior saldo
var maiorSaldo = ConsultaUtils.MaiorPor(repoContas.ListarTodos(), c => c.Saldo);
Console.WriteLine($"Maior saldo: {maiorSaldo?.ExibirExtrato()}");

// Contas agrupadas por tipo
var porTipo = ConsultaUtils.AgruparPor(repoContas.ListarTodos(), c => c.GetType().Name);
foreach (var (tipo, contas) in porTipo)
    Console.WriteLine($"{tipo}: {contas.Count} conta(s)");
```

---

## Exercicios

1. Adicione um metodo generico `ExportarParaCsv<T>(IRepositorio<T> repo, Func<T, string> formatador)` que imprime todas as entidades em formato CSV.
2. Crie uma interface `IProcessador<in T>` (contravariante) e demonstre que `IProcessador<IConta>` pode ser usado onde se espera `IProcessador<ContaCorrente>`.
3. Adicione constraint `where T : IIdentificavel, new()` e implemente um metodo `CriarPadrao()` no repositorio.
4. Crie `RepositorioComAuditoria<T>` que herda de `RepositorioEmMemoria<T>` e registra log a cada operacao.
