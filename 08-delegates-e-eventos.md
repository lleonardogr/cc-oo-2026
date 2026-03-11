# Aula 8 - Delegates e Eventos em Profundidade

## Teoria

Um **delegate** e um tipo que guarda referencia a um metodo. Quando invocado, o metodo referenciado executa. O livro-base compara com um representante diplomatico: faz o trabalho no lugar de quem o designou.

**Por que usar?** Chamada direta cria acoplamento. Delegate permite decidir em runtime qual metodo chamar. **Multicasting** (`+=`/`-=`) encadeia multiplos metodos. **Eventos** sao delegates protegidos: codigo externo so inscreve/desinscreve.

### Delegates pre-definidos

- `Action<T>` — sem retorno
- `Func<T, TResult>` — com retorno
- `Predicate<T>` — retorna `bool`

### Lambdas

```csharp
Func<int, int, int> somar = (a, b) => a + b;
```

---

## 🏦 Hands-on: App Bancario — Pipeline de notificacoes e validacao

Vamos sofisticar o sistema de notificacoes e adicionar um pipeline de validacao de transacoes usando delegates.

### Passo 1: Pipeline de validacao com delegates

Antes de executar uma transacao, queremos rodar multiplas validacoes (saldo, limite diario, conta ativa). Cada validacao e um delegate:

```csharp
// === MiniBank v0.7 — Delegates em profundidade ===

public delegate bool ValidacaoTransacao(IConta conta, decimal valor, out string motivo);

public class ValidadorTransacoes
{
    private readonly List<ValidacaoTransacao> validacoes = new();

    public void Adicionar(ValidacaoTransacao validacao) => validacoes.Add(validacao);

    public bool ValidarTodas(IConta conta, decimal valor)
    {
        foreach (var validacao in validacoes)
        {
            if (!validacao(conta, valor, out string motivo))
            {
                Console.WriteLine($"[VALIDACAO FALHOU] {motivo}");
                return false;
            }
        }
        return true;
    }
}
```

Validacoes individuais:

```csharp
public static class Validacoes
{
    public static bool ValidarSaldo(IConta conta, decimal valor, out string motivo)
    {
        motivo = "";
        if (valor > conta.Saldo)
        {
            motivo = $"Saldo insuficiente ({conta.Saldo:C} < {valor:C})";
            return false;
        }
        return true;
    }

    public static bool ValidarValorPositivo(IConta conta, decimal valor, out string motivo)
    {
        motivo = "";
        if (valor <= 0)
        {
            motivo = "Valor deve ser positivo";
            return false;
        }
        return true;
    }

    public static bool ValidarLimiteDiario(IConta conta, decimal valor, out string motivo)
    {
        motivo = "";
        decimal limiteDiario = 10_000m;
        if (valor > limiteDiario)
        {
            motivo = $"Valor excede limite diario de {limiteDiario:C}";
            return false;
        }
        return true;
    }
}
```

Uso:

```csharp
var validador = new ValidadorTransacoes();
validador.Adicionar(Validacoes.ValidarValorPositivo);
validador.Adicionar(Validacoes.ValidarSaldo);
validador.Adicionar(Validacoes.ValidarLimiteDiario);

if (validador.ValidarTodas(ccAna, 500m))
{
    ccAna.Sacar(500m);
    Console.WriteLine("Saque realizado!");
}
```

### Passo 2: Notificacoes com Action

Simplificamos as notificacoes usando `Action<TransacaoEventArgs>`:

```csharp
public class CentralNotificacoes
{
    private readonly List<Action<TransacaoEventArgs>> handlers = new();

    public void Inscrever(Action<TransacaoEventArgs> handler) => handlers.Add(handler);
    public void Desinscrever(Action<TransacaoEventArgs> handler) => handlers.Remove(handler);

    public void Notificar(TransacaoEventArgs args)
    {
        foreach (var handler in handlers)
            handler(args);
    }
}
```

Registrando com lambdas:

```csharp
var central = new CentralNotificacoes();

// Log simples
central.Inscrever(e =>
    Console.WriteLine($"[LOG] {e.Transacao.Tipo}: {e.Transacao.Valor:C} na conta {e.Conta.Numero}"));

// Email
central.Inscrever(e =>
    Console.WriteLine($"[EMAIL -> {e.Conta.Titular.Email}] Movimentacao de {e.Transacao.Valor:C}"));

// SMS para valores altos
central.Inscrever(e =>
{
    if (e.Transacao.Valor > 1000m)
        Console.WriteLine($"[SMS] Alerta: movimentacao acima de R$1.000 na conta {e.Conta.Numero}");
});
```

### Passo 3: Conectando na ContaBase

```csharp
public abstract class ContaBase : IConta, IIdentificavel
{
    // ... propriedades anteriores ...
    public CentralNotificacoes? Notificacoes { get; set; }

    protected void NotificarTransacao(Transacao transacao)
    {
        Extrato.Registrar(transacao);
        Notificacoes?.Notificar(new TransacaoEventArgs(transacao, this));
    }
}
```

### Passo 4: Func para calculo dinamico

Podemos tambem usar `Func` para logica que retorna valor:

```csharp
// Calculo de rendimento customizavel via Func
public class ContaPoupancaFlex : ContaBase
{
    private readonly Func<decimal, decimal> calculoRendimento;

    public ContaPoupancaFlex(string numero, Cliente titular, decimal saldo, Func<decimal, decimal> calculo)
        : base(numero, titular, saldo)
    {
        calculoRendimento = calculo;
    }

    public void AplicarRendimento()
    {
        decimal rendimento = calculoRendimento(Saldo);
        Depositar(rendimento);
    }

    public override bool Sacar(decimal valor)
    {
        if (valor <= 0 || valor > Saldo) return false;
        Saldo -= valor;
        NotificarTransacao(new Transacao(valor, TipoTransacao.Saque, "Saque poupanca"));
        return true;
    }
}

// Uso com lambda:
var cpFlex = new ContaPoupancaFlex("CP-FLEX-001", ana, 10000m,
    saldo => saldo > 5000 ? saldo * 0.01m : saldo * 0.005m);
cpFlex.AplicarRendimento(); // rendimento de 1% (saldo > 5000)
```

### Teste integrado

```csharp
var central = new CentralNotificacoes();
central.Inscrever(e => Console.WriteLine($"[LOG] {e.Transacao}"));

var ana = new Cliente("Ana Silva", "123.456.789-00", "ana@email.com");
var cc = new ContaCorrente("CC-001", ana, 5000m);
cc.Notificacoes = central;

var validador = new ValidadorTransacoes();
validador.Adicionar(Validacoes.ValidarValorPositivo);
validador.Adicionar(Validacoes.ValidarSaldo);

if (validador.ValidarTodas(cc, 1500m))
{
    cc.Sacar(1500m);
}
// [LOG] 11/03/2026 ... | Saque | R$ 1.500,00 | Saque
```

---

## Exercicios

1. Adicione uma validacao `ValidarHorarioComercial` que rejeita transacoes fora do horario 8h-18h.
2. Crie um `Func<IConta, string>` para formatar extrato em diferentes formatos (texto, CSV, JSON). Teste com ao menos dois formatos.
3. Implemente um sistema de "callbacks de confirmacao": antes de executar o saque, chame um `Func<decimal, bool>` que pede confirmacao.
