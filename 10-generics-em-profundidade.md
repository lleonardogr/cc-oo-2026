# Aula 10 - Generics em Profundidade

## Objetivo da aula

Aprofundar generics de verdade, usando constraints, variancia e utilitarios genericos sem transformar a aula em mera repeticao do tema repositorio.

## Pre-requisitos

- dominar a versao `v0.8`
- entender o `Repositorio<T>` introdutorio da Aula 6
- compreender repositorios por interface da Aula 9

## Ao final, o aluno sera capaz de...

- explicar por que generics evitam `object`, casting e duplicacao
- usar constraints para definir contratos de tipo
- reconhecer quando um repositorio generico ajuda e quando complica
- demonstrar covariancia e contravariancia com exemplos concretos

## Teoria essencial

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

## Erros e confusoes comuns

- achar que generic sempre significa melhor design
- usar `T` sem explicar qual contrato esse tipo precisa cumprir
- confundir repositorio generico com "arquitetura pronta"
- decorar `in` e `out` sem entender restricao de entrada e saida

---

## 🏦 Hands-on: App Bancario — Repositorio generico unificado

### Estado atual do MiniBank

- Versao de entrada: `v0.8`
- Versao de saida: `v0.9`
- Classes novas: `IRepositorio<T>`, `RepositorioEmMemoria<T>`, `ConsultaUtils`, `IFabricaConta<out T>`
- Classes alteradas: entidades passam a assumir com mais clareza o contrato `IIdentificavel`
- Comportamentos novos: busca generica, especializacao por heranca generica, utilitarios genricos e covariancia
- Como testar no Main: adicionar entidades aos repositorios, consultar por filtro, agrupar e criar conta via fabrica covariante

### O que muda nesta aula

O aluno volta a generics, mas agora para entender limites e vantagens do recurso em profundidade.

### Por que muda

Depois de usar interfaces e DI, fica mais claro onde um mecanismo generico ajuda e onde o dominio ainda precisa de especializacao.

### Organizando o projeto

1. Reaproveite a pasta `Repositories` e crie `Repositories/Generic`.
2. Coloque `IRepositorio.cs` e `RepositorioEmMemoria.cs` em `Repositories/Generic`.
3. Se mantiver um repositorio especializado para contas, coloque-o em `Repositories/Specialized/RepositorioConta.cs`.
4. Crie a pasta `Utils` para `ConsultaUtils.cs`.
5. Crie a pasta `Factories` para `IFabricaConta.cs` e `FabricaContaCorrente.cs`.

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

## Checklist de verificacao da versao

- o repositorio generico usa constraint coerente com o dominio
- `Buscar`, `AgruparPor` e `MaiorPor` funcionam com tipos diferentes
- o aluno consegue explicar por que a Aula 10 e aprofundamento de generics, nao apenas repeticao de repositorio
- a covariancia e demonstrada com `IFabricaConta<out T>`
- o uso de especializacao por heranca generica nao quebra o contrato do repositorio

## Exercicios

1. Adicione um metodo generico `ExportarParaCsv<T>(IRepositorio<T> repo, Func<T, string> formatador)` que imprime todas as entidades em formato CSV.
2. Crie uma interface `IProcessador<in T>` (contravariante) e demonstre que `IProcessador<IConta>` pode ser usado onde se espera `IProcessador<ContaCorrente>`.
3. Adicione constraint `where T : IIdentificavel, new()` e implemente um metodo `CriarPadrao()` no repositorio.
4. Crie `RepositorioComAuditoria<T>` que herda de `RepositorioEmMemoria<T>` e registra log a cada operacao.

### Gabarito comentado

1. Implementacao de referencia:

```csharp
public static void ExportarParaCsv<T>(IRepositorio<T> repo, Func<T, string> formatador)
    where T : IIdentificavel
{
    foreach (var item in repo.ListarTodos())
        Console.WriteLine(formatador(item));
}
```

Como verificar:
- passar um formatador para `Cliente` e outro para `ContaBase`
- observar linhas CSV diferentes sem mudar o metodo generico

2. Implementacao de referencia:

```csharp
public interface IProcessador<in T>
{
    void Processar(T item);
}

public class ProcessadorConta : IProcessador<IConta>
{
    public void Processar(IConta item)
        => Console.WriteLine(item.ExibirExtrato());
}

IProcessador<IConta> processadorGeral = new ProcessadorConta();
IProcessador<ContaCorrente> processadorCc = processadorGeral;
```

3. Implementacao de referencia:

```csharp
public class RepositorioEmMemoria<T> : IRepositorio<T>
    where T : IIdentificavel, new()
{
    public T CriarPadrao()
    {
        var novo = new T();
        Adicionar(novo);
        return novo;
    }
}
```

Criterio de aceitacao:
- a constraint `new()` aparece no tipo
- `CriarPadrao()` consegue instanciar `T`

4. Implementacao de referencia:

```csharp
public class RepositorioComAuditoria<T> : RepositorioEmMemoria<T>
    where T : IIdentificavel
{
    public new void Adicionar(T item)
    {
        Console.WriteLine($"[AUDIT] Adicionando {item.Id}");
        base.Adicionar(item);
    }
}
```

Sinal de que ficou correto:
- o log aparece antes da operacao
- o comportamento base continua funcionando

Erros comuns:
- esquecer a constraint no metodo CSV
- inverter `in` e `out`
- usar `new T()` sem declarar `where T : new()`
- criar auditoria que reimplementa tudo em vez de reaproveitar a base

## Fechamento e conexao com a proxima aula

Agora o aluno viu que generics servem para generalizar com seguranca, mas tambem exigem criterio. A Aula 11 fecha a trilha conectando esse design a recursos modernos de `C#` que fortalecem contratos e legibilidade.

### Versao esperada apos esta aula

- Versao de entrada: `v0.8`
- Versao de saida: `v0.9`
- Classes novas: repositorio generico robusto, utilitarios genericos e fabrica covariante
- Classes alteradas: entidades reforcam o contrato `IIdentificavel`
- Comportamentos novos: filtros, agrupamentos, especializacao generica e demonstracao de variancia
- Como testar no Main: listar, buscar, agrupar e criar contas via contratos genericos
