# Aula 9 - Interfaces: DI, Testabilidade e Composicao

## Teoria

Interfaces sao o mecanismo central para **desacoplamento**, **testabilidade** e **extensibilidade**. A Aula 3 apresentou o basico; aqui aprofundamos com injecao de dependencia, implementacao explicita e testes.

### Interface vs Classe abstrata

| Criterio | Interface | Classe Abstrata |
|----------|-----------|-----------------|
| Implementacao | Nao (classica) | Sim |
| Heranca multipla | Sim | Nao |
| Estado (campos) | Nao | Sim |
| Quando usar | Contrato puro, DI | Base com logica compartilhada |

### Injecao de Dependencia (DI)

Em vez de uma classe criar suas dependencias, ela recebe abstracoes via construtor. Isso permite trocar implementacoes sem alterar o consumidor.

---

## 🏦 Hands-on: App Bancario — Persistencia e testes via interfaces

Vamos aplicar DI para permitir trocar a persistencia do MiniBank, e criar testes simples sem banco de dados.

### Passo 1: Interfaces de repositorio

```csharp
// === MiniBank v0.8 — Interfaces e DI ===

public interface IRepositorioCliente
{
    void Salvar(Cliente cliente);
    Cliente? BuscarPorCpf(string cpf);
    IEnumerable<Cliente> ListarTodos();
}

public interface IRepositorioConta
{
    void Salvar(IConta conta);
    IConta? BuscarPorNumero(string numero);
    IEnumerable<IConta> ListarTodas();
    IEnumerable<IConta> BuscarPorCliente(string cpf);
}
```

### Passo 2: Implementacao em memoria

```csharp
public class RepositorioContaEmMemoria : IRepositorioConta
{
    private readonly List<IConta> contas = new();

    public void Salvar(IConta conta)
    {
        var existente = BuscarPorNumero(conta.Numero);
        if (existente != null)
            contas.Remove(existente);
        contas.Add(conta);
    }

    public IConta? BuscarPorNumero(string numero)
        => contas.FirstOrDefault(c => c.Numero == numero);

    public IEnumerable<IConta> ListarTodas() => contas;

    public IEnumerable<IConta> BuscarPorCliente(string cpf)
        => contas.Where(c => c.Titular.Cpf == cpf);
}

public class RepositorioClienteEmMemoria : IRepositorioCliente
{
    private readonly List<Cliente> clientes = new();

    public void Salvar(Cliente cliente)
    {
        if (!clientes.Any(c => c.Cpf == cliente.Cpf))
            clientes.Add(cliente);
    }

    public Cliente? BuscarPorCpf(string cpf)
        => clientes.FirstOrDefault(c => c.Cpf == cpf);

    public IEnumerable<Cliente> ListarTodos() => clientes;
}
```

### Passo 3: Servico de negocio com DI

```csharp
public class ServicoBancario
{
    private readonly IRepositorioCliente repoClientes;
    private readonly IRepositorioConta repoContas;
    private readonly ICalculadoraTaxa calculadoraTaxa;
    private int contadorContas = 0;

    public ServicoBancario(
        IRepositorioCliente repoClientes,
        IRepositorioConta repoContas,
        ICalculadoraTaxa calculadoraTaxa)
    {
        this.repoClientes = repoClientes;
        this.repoContas = repoContas;
        this.calculadoraTaxa = calculadoraTaxa;
    }

    public Cliente CadastrarCliente(string nome, string cpf, string email)
    {
        if (repoClientes.BuscarPorCpf(cpf) != null)
            throw new InvalidOperationException("Cliente ja cadastrado.");

        var cliente = new Cliente(nome, cpf, email);
        repoClientes.Salvar(cliente);
        return cliente;
    }

    public ContaCorrente AbrirContaCorrente(Cliente cliente, decimal saldoInicial = 0)
    {
        var conta = new ContaCorrente($"CC-{++contadorContas:D4}", cliente, saldoInicial);
        repoContas.Salvar(conta);
        return conta;
    }

    public bool Transferir(string numeroOrigem, string numeroDestino, decimal valor)
    {
        var origem = repoContas.BuscarPorNumero(numeroOrigem)
            ?? throw new InvalidOperationException($"Conta {numeroOrigem} nao encontrada.");
        var destino = repoContas.BuscarPorNumero(numeroDestino)
            ?? throw new InvalidOperationException($"Conta {numeroDestino} nao encontrada.");

        decimal taxa = calculadoraTaxa.Calcular(valor);
        if (!origem.Sacar(valor + taxa)) return false;
        destino.Depositar(valor);
        return true;
    }
}
```

### Passo 4: Montagem (composicao raiz)

```csharp
// Composicao raiz — onde decidimos quais implementacoes usar
var repoClientes = new RepositorioClienteEmMemoria();
var repoContas = new RepositorioContaEmMemoria();
var taxa = new TaxaContaCorrente(); // 2%

var servico = new ServicoBancario(repoClientes, repoContas, taxa);

var ana = servico.CadastrarCliente("Ana Silva", "123.456.789-00", "ana@email.com");
var joao = servico.CadastrarCliente("Joao Santos", "987.654.321-00", "joao@email.com");

var ccAna = servico.AbrirContaCorrente(ana, 5000m);
var ccJoao = servico.AbrirContaCorrente(joao, 1000m);

servico.Transferir(ccAna.Numero, ccJoao.Numero, 1000m);
```

### Passo 5: Testabilidade

Sem framework de teste — apenas metodos que verificam comportamento:

```csharp
public static class TesteServicoBancario
{
    public static void TestarTransferenciaComSucesso()
    {
        // Arrange
        var repoClientes = new RepositorioClienteEmMemoria();
        var repoContas = new RepositorioContaEmMemoria();
        var taxa = new TaxaContaPoupanca(); // 0% para simplificar
        var servico = new ServicoBancario(repoClientes, repoContas, taxa);

        var cli1 = servico.CadastrarCliente("A", "111.111.111-11", "a@a.com");
        var cli2 = servico.CadastrarCliente("B", "222.222.222-22", "b@b.com");
        var cc1 = servico.AbrirContaCorrente(cli1, 1000m);
        var cc2 = servico.AbrirContaCorrente(cli2, 0m);

        // Act
        bool resultado = servico.Transferir(cc1.Numero, cc2.Numero, 300m);

        // Assert
        Debug.Assert(resultado == true, "Transferencia deveria ter sucesso");
        Debug.Assert(cc1.Saldo == 700m, $"Saldo origem esperado 700, obtido {cc1.Saldo}");
        Debug.Assert(cc2.Saldo == 300m, $"Saldo destino esperado 300, obtido {cc2.Saldo}");
        Console.WriteLine("✅ TestarTransferenciaComSucesso PASSOU");
    }

    public static void TestarTransferenciaSemSaldo()
    {
        var repoClientes = new RepositorioClienteEmMemoria();
        var repoContas = new RepositorioContaEmMemoria();
        var taxa = new TaxaContaPoupanca();
        var servico = new ServicoBancario(repoClientes, repoContas, taxa);

        var cli = servico.CadastrarCliente("C", "333.333.333-33", "c@c.com");
        var cc1 = servico.AbrirContaCorrente(cli, 100m);
        var cc2 = servico.AbrirContaCorrente(cli, 0m);

        bool resultado = servico.Transferir(cc1.Numero, cc2.Numero, 99999m);

        Debug.Assert(resultado == false, "Deveria falhar por saldo insuficiente");
        Console.WriteLine("✅ TestarTransferenciaSemSaldo PASSOU");
    }
}

// No Main:
TesteServicoBancario.TestarTransferenciaComSucesso();
TesteServicoBancario.TestarTransferenciaSemSaldo();
```

Note: os testes nao dependem de banco de dados, rede ou configuracao. Usam repositorios em memoria. Se amanha trocarmos para SQL Server, os testes continuam rodando com o fake.

### Diagrama da DI

```mermaid
flowchart TD
    SB[ServicoBancario] --> IRC[IRepositorioConta]
    SB --> IRCL[IRepositorioCliente]
    SB --> ICT[ICalculadoraTaxa]
    MEM1[RepoContaEmMemoria] -.-> IRC
    MEM2[RepoClienteEmMemoria] -.-> IRCL
    SQL1[RepoContaSql - futuro] -.-> IRC
    TX[TaxaContaCorrente] -.-> ICT
```

---

## Exercicios

1. Crie `IServicoNotificacao` com metodo `Notificar(string destinatario, string mensagem)`. Implemente `NotificacaoConsole` e injete no `ServicoBancario`.
2. Escreva um teste que verifica se cadastrar cliente duplicado lanca excecao.
3. Crie `IRelatorioServico` e implemente um `RelatorioConsole` que lista todas as contas e saldos. Injete no servico.
4. Implemente `RepositorioContaEmArquivo : IRepositorioConta` que salva contas em arquivo texto. Troque na composicao raiz e observe que o `ServicoBancario` nao muda.
