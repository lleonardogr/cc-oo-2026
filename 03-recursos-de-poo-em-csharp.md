# Aula 3 - Recursos de POO em C#

## Teoria

O EPUB usa recursos especificos de `C#` para materializar os pilares: interfaces, classes abstratas, parciais, seladas, propriedades e modificadores de acesso.

### Interfaces — revisao

Interface = contrato puro. Nao pode ser instanciada. Uma classe pode implementar multiplas interfaces. Se nao implementar todos os membros, o compilador gera erro.

### Classes abstratas

Definem base comum e podem conter implementacao pronta. Nao podem ser instanciadas. Metodos `abstract` obrigam a derivada a fornecer implementacao.

### Classes seladas (`sealed`)

Nao podem ser herdadas. Usadas quando extensao nao faz sentido para o design.

### Classes parciais (`partial`)

Permitem dividir uma classe em varios arquivos. Util para codigo gerado automaticamente.

### Propriedades com validacao

```csharp
private decimal preco;
public decimal Preco
{
    get => preco;
    set => preco = value >= 0 ? value : throw new ArgumentException("Preco invalido.");
}
```

### Modificadores de acesso

| Modificador | Acesso |
|-------------|--------|
| `public` | Qualquer lugar |
| `private` | Somente dentro da classe |
| `protected` | Classe e derivadas |
| `internal` | Mesmo assembly |
| `protected internal` | Assembly ou derivadas |

### Quando usar o que

```mermaid
flowchart TD
    A[Preciso apenas de contrato?] -->|Sim| B[Interface]
    A -->|Nao| C[Preciso compartilhar codigo base?]
    C -->|Sim| D[Classe abstrata]
    C -->|Nao| E{Quero impedir heranca?}
    E -->|Sim| F[Classe selada]
    E -->|Nao| G[Classe normal]
```

---

## 🏦 Hands-on: App Bancario — Interfaces multiplas e propriedades robustas

Na v0.2 temos hierarquia de contas. Agora vamos adicionar **interfaces segregadas**, uma **classe selada** para configuracao e **propriedades com validacao real**.

### Passo 1: Interfaces segregadas

Nem todo componente precisa de todas as operacoes. Vamos segregar:

```csharp
// === MiniBank v0.3 — Interfaces e propriedades ===

public interface IDebitavel
{
    bool Sacar(decimal valor);
}

public interface ICreditavel
{
    void Depositar(decimal valor);
}

public interface IExibivel
{
    string ExibirExtrato();
}

// IConta agora compoe as interfaces menores
public interface IConta : IDebitavel, ICreditavel, IExibivel
{
    string Numero { get; }
    decimal Saldo { get; }
    Cliente Titular { get; }
}
```

Codigo que so precisa creditar pode depender apenas de `ICreditavel`. Codigo que gera relatorio depende de `IExibivel`. Isso e Interface Segregation.

### Passo 2: Propriedades com validacao no `Cliente`

```csharp
public class Cliente
{
    public string Nome { get; private set; }
    public string Cpf { get; private set; }

    private string email = "";
    public string Email
    {
        get => email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
                throw new ArgumentException("Email invalido.");
            email = value;
        }
    }

    public Cliente(string nome, string cpf, string email)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome obrigatorio.");
        if (string.IsNullOrWhiteSpace(cpf)) throw new ArgumentException("CPF obrigatorio.");
        Nome = nome;
        Cpf = cpf;
        Email = email; // passa pela validacao do setter
    }

    public override string ToString() => $"{Nome} ({Cpf})";
}
```

### Passo 3: Classe selada para configuracao

```csharp
public sealed class ConfiguracaoBanco
{
    public string NomeBanco { get; } = "MiniBank";
    public decimal TaxaTransferencia { get; } = 5.00m;
    public decimal LimitePadraoCC { get; } = 500m;
    public decimal TaxaRendimentoPoupanca { get; } = 0.005m;
}

// Nao pode herdar:
// public class ConfiguracaoEspecial : ConfiguracaoBanco { } // Erro!
```

### Passo 4: `ContaBase` protegendo o Saldo

```csharp
public abstract class ContaBase : IConta
{
    public string Numero { get; }
    public Cliente Titular { get; }

    private decimal saldo;
    public decimal Saldo
    {
        get => saldo;
        protected set
        {
            // Conta corrente pode ficar negativa (cheque especial), mas com limite
            saldo = value;
        }
    }

    protected ContaBase(string numero, Cliente titular, decimal saldoInicial)
    {
        if (saldoInicial < 0) throw new ArgumentException("Saldo inicial nao pode ser negativo.");
        Numero = numero;
        Titular = titular;
        saldo = saldoInicial;
    }

    public void Depositar(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor deve ser positivo.");
        Saldo += valor;
    }

    public abstract bool Sacar(decimal valor);

    public virtual string ExibirExtrato()
        => $"[{GetType().Name}] {Numero} | {Titular.Nome} | Saldo: {Saldo:C}";
}
```

### Testando

```csharp
var config = new ConfiguracaoBanco();

var ana = new Cliente("Ana Silva", "123.456.789-00", "ana@email.com");
IConta cc = new ContaCorrente("CC-001", ana, 1000m, config.LimitePadraoCC);
IConta cp = new ContaPoupanca("CP-001", ana, 2000m, config.TaxaRendimentoPoupanca);

// Usando interface segregada:
IExibivel exibivel = cc;
Console.WriteLine(exibivel.ExibirExtrato());

// Validacao em acao:
try { new Cliente("", "123", "invalido"); }
catch (ArgumentException ex) { Console.WriteLine($"Erro: {ex.Message}"); }

try { cc.Depositar(-100m); }
catch (ArgumentException ex) { Console.WriteLine($"Erro: {ex.Message}"); }
```

### Diagrama atualizado

```mermaid
classDiagram
    class IDebitavel { <<interface>> +Sacar(decimal) bool }
    class ICreditavel { <<interface>> +Depositar(decimal) }
    class IExibivel { <<interface>> +ExibirExtrato() string }
    class IConta { <<interface>> }

    IConta --|> IDebitavel
    IConta --|> ICreditavel
    IConta --|> IExibivel

    class ContaBase { <<abstract>> }
    class ContaCorrente
    class ContaPoupanca
    class ConfiguracaoBanco { <<sealed>> }

    IConta <|.. ContaBase
    ContaBase <|-- ContaCorrente
    ContaBase <|-- ContaPoupanca
```

---

## Exercicios

1. Crie uma interface `ITransferivel` com metodo `Transferir(IConta destino, decimal valor)`. Implemente em `ContaCorrente` (que cobra taxa de R$5) e `ContaPoupanca` (sem taxa).
2. Tente criar uma classe que herde de `ConfiguracaoBanco` e observe o erro do compilador.
3. Adicione validacao de CPF (exatamente 14 caracteres com formato `XXX.XXX.XXX-XX`).
