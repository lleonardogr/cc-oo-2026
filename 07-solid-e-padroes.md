# Aula 7 - SOLID e Padroes de Projeto

## Teoria

Os principios SOLID (Robert C. Martin) guiam a escrita de codigo limpo e extensivel:

- **S** — Single Responsibility: uma classe, uma razao para mudar
- **O** — Open/Closed: aberto para extensao, fechado para modificacao
- **L** — Liskov Substitution: subclasses respeitam o contrato da base
- **I** — Interface Segregation: interfaces pequenas e focadas
- **D** — Dependency Inversion: dependa de abstracoes, nao de concretos

### Padrao Strategy

Encapsula familias de algoritmos em classes separadas, tornando-os intercambiaveis. Combina polimorfismo com Open/Closed.

---

## 🏦 Hands-on: App Bancario — Aplicando SOLID e Strategy

Vamos refatorar o MiniBank para seguir SOLID e introduzir o padrao Strategy para calculo de taxas.

### SRP: Separar `ServicoTransferencia`

Na v0.4, a transferencia estava dentro de `ContaCorrente`. Isso viola SRP — a conta nao deveria saber sobre a logica de transferencia entre contas.

```csharp
// === MiniBank v0.6 — SOLID e Strategy ===

public class ServicoTransferencia
{
    public bool Executar(IConta origem, IConta destino, decimal valor)
    {
        if (origem.Numero == destino.Numero)
            throw new InvalidOperationException("Nao pode transferir para a mesma conta.");

        if (!origem.Sacar(valor)) return false;
        destino.Depositar(valor);
        return true;
    }
}
```

Agora a conta cuida do seu saldo; o servico cuida da orquestracao.

### OCP + Strategy: Calculo de taxas intercambiavel

O banco cobra taxas diferentes em operacoes. Em vez de usar `if/else` por tipo de conta:

```csharp
// RUIM: viola OCP — cada nova taxa exige alterar o metodo
public decimal CalcularTaxa(IConta conta, decimal valor)
{
    if (conta is ContaCorrente) return valor * 0.02m;
    if (conta is ContaPoupanca) return 0m;
    // cada novo tipo exige mais ifs...
    return 0;
}
```

Usamos Strategy:

```csharp
public interface ICalculadoraTaxa
{
    decimal Calcular(decimal valor);
}

public class TaxaContaCorrente : ICalculadoraTaxa
{
    public decimal Calcular(decimal valor) => valor * 0.02m; // 2%
}

public class TaxaContaPoupanca : ICalculadoraTaxa
{
    public decimal Calcular(decimal valor) => 0m; // isenta
}

public class TaxaContaPremium : ICalculadoraTaxa
{
    public decimal Calcular(decimal valor) => valor > 10_000 ? 0m : valor * 0.01m;
}
```

Servico de transferencia com taxa:

```csharp
public class ServicoTransferencia
{
    private readonly ICalculadoraTaxa calculadoraTaxa;

    public ServicoTransferencia(ICalculadoraTaxa calculadoraTaxa)
    {
        this.calculadoraTaxa = calculadoraTaxa;
    }

    public bool Executar(IConta origem, IConta destino, decimal valor)
    {
        decimal taxa = calculadoraTaxa.Calcular(valor);
        decimal total = valor + taxa;

        if (!origem.Sacar(total)) return false;
        destino.Depositar(valor);

        Console.WriteLine($"Transferencia: {valor:C} + taxa {taxa:C} = {total:C}");
        return true;
    }
}
```

Para adicionar um novo tipo de taxa, basta criar uma classe nova. Nenhum codigo existente muda.

### DIP: Servicos dependem de abstracoes

```csharp
public interface IRepositorioConta
{
    void Salvar(IConta conta);
    IConta? BuscarPorNumero(string numero);
    IEnumerable<IConta> ListarTodas();
}

public class ServicoConta
{
    private readonly IRepositorioConta repositorio;

    public ServicoConta(IRepositorioConta repositorio)
    {
        this.repositorio = repositorio;
    }

    public IConta? Buscar(string numero) => repositorio.BuscarPorNumero(numero);
    public void Salvar(IConta conta) => repositorio.Salvar(conta);
}
```

`ServicoConta` nao sabe se os dados estao em memoria, SQL Server ou arquivo. Depende apenas da interface.

### LSP: ContaPoupanca respeita o contrato

Se substituirmos `ContaBase` por `ContaPoupanca` em qualquer lugar que espere uma `IConta`, o comportamento deve continuar correto. `Depositar` soma, `Sacar` valida saldo. O contrato e mantido.

### ISP: Ja aplicamos na Aula 3

`IDebitavel`, `ICreditavel` e `IExibivel` sao interfaces segregadas.

### Diagrama do Strategy

```mermaid
classDiagram
    class ICalculadoraTaxa {
        <<interface>>
        +Calcular(decimal) decimal
    }
    class TaxaContaCorrente { +Calcular(decimal) decimal }
    class TaxaContaPoupanca { +Calcular(decimal) decimal }
    class TaxaContaPremium { +Calcular(decimal) decimal }
    class ServicoTransferencia {
        -ICalculadoraTaxa taxa
        +Executar(IConta, IConta, decimal) bool
    }

    ICalculadoraTaxa <|.. TaxaContaCorrente
    ICalculadoraTaxa <|.. TaxaContaPoupanca
    ICalculadoraTaxa <|.. TaxaContaPremium
    ServicoTransferencia --> ICalculadoraTaxa
```

### Testando

```csharp
var ana = new Cliente("Ana Silva", "123.456.789-00", "ana@email.com");
var joao = new Cliente("Joao Santos", "987.654.321-00", "joao@email.com");

var ccAna = new ContaCorrente("CC-001", ana, 5000m);
var ccJoao = new ContaCorrente("CC-002", joao, 1000m);

// Transferencia com taxa de conta corrente (2%)
var servico = new ServicoTransferencia(new TaxaContaCorrente());
servico.Executar(ccAna, ccJoao, 1000m);
// Transferencia: R$ 1.000,00 + taxa R$ 20,00 = R$ 1.020,00

Console.WriteLine(ccAna.ExibirExtrato());  // Saldo: 3980
Console.WriteLine(ccJoao.ExibirExtrato()); // Saldo: 2000

// Mesma logica com taxa premium:
var servicoPremium = new ServicoTransferencia(new TaxaContaPremium());
servicoPremium.Executar(ccAna, ccJoao, 500m);
// Transferencia: R$ 500,00 + taxa R$ 5,00 = R$ 505,00
```

---

## Exercicios

1. Crie `TaxaTransferenciaGratuita` que isenta transferencias acima de R$5.000.
2. Refatore o `Banco` (Aula 4) para receber `IRepositorioConta` via construtor (DIP).
3. Identifique no MiniBank atual se alguma classe ainda viola SRP. Refatore.
4. Crie um `ServicoRelatorio` que dependa de `IRepositorioConta` e gere um resumo do banco.
