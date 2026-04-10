# Aula 8 - Delegates e Eventos em Profundidade

## Objetivo da aula

Aprofundar o mecanismo por tras dos eventos introduzidos na Aula 6, usando delegates, lambdas e funcoes de ordem superior de forma mais consciente.

## Pre-requisitos

- dominar a versao `v0.6`
- ter entendido o uso de `event EventHandler<T>` da Aula 6
- saber ler lambdas simples em `C#`

## Ao final, o aluno sera capaz de...

- explicar a diferenca entre delegate, evento e lambda
- criar pipelines de validacao com delegates customizados
- usar `Action` e `Func` em cenarios de extensao do `MiniBank`
- reconhecer que esta aula aprofunda um mecanismo ja introduzido, em vez de reiniciar o tema

## Teoria essencial

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

## Erros e confusoes comuns

- achar que delegate e "evento com outro nome"
- usar lambda quando um metodo nomeado seria mais claro
- trocar o evento idiomatico da Aula 6 por qualquer lista de `Action` sem perceber o tradeoff
- esquecer de explicar por que esta aula e aprofundamento, nao repeticao

---

## 🏦 Hands-on: App Bancario — Pipeline de notificacoes e validacao

### Estado atual do MiniBank

- Versao de entrada: `v0.6`
- Versao de saida: `v0.7`
- Classes novas: `ValidadorTransacoes`, `Validacoes`, `CentralNotificacoes`, `ContaPoupancaFlex`
- Classes alteradas: `ContaBase`
- Comportamentos novos: pipeline de validacao, notificacao por `Action`, calculo dinamico via `Func`
- Como testar no Main: registrar validacoes, inscrever handlers e aplicar rendimento com lambda customizada

### O que muda nesta aula

Saindo do evento idiomatico da Aula 6, agora o aluno ve o mecanismo subjacente com mais flexibilidade e mais responsabilidade de design.

### Por que muda

Esse segundo contato ajuda a entender que `event` e uma aplicacao controlada de delegates, e nao uma "mecanica magica" de `C#`.

### Organizando o projeto

1. Crie a pasta `Validation` para os componentes de validacao da aula.
2. Adicione os arquivos `Validation/ValidadorTransacoes.cs` e `Validation/Validacoes.cs`.
3. Na pasta `Services`, crie `CentralNotificacoes.cs`.
4. Se optar por deixar o delegate nomeado em arquivo proprio, crie `Validation/ValidacaoTransacao.cs`.
5. Em `Models/Contas`, adicione `ContaPoupancaFlex.cs` apenas se quiser manter o exemplo separado da `ContaPoupanca` principal.

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

## Checklist de verificacao da versao

- ha pelo menos um delegate customizado para validacao
- `Action` e `Func` aparecem em cenarios em que seu uso faz sentido
- o aluno consegue comparar o evento idiomatico da Aula 6 com a central baseada em `Action`
- as validacoes sao compostas sem `if` gigante no metodo chamador
- o fluxo de notificacao continua observavel no `Main`

## Exercicios

1. Adicione uma validacao `ValidarHorarioComercial` que rejeita transacoes fora do horario 8h-18h.
2. Crie um `Func<IConta, string>` para formatar extrato em diferentes formatos (texto, CSV, JSON). Teste com ao menos dois formatos.
3. Implemente um sistema de "callbacks de confirmacao": antes de executar o saque, chame um `Func<decimal, bool>` que pede confirmacao.

### Gabarito comentado

1. Implementacao de referencia:

```csharp
public static bool ValidarHorarioComercial(IConta conta, decimal valor, out string motivo)
{
    motivo = "";
    int hora = DateTime.Now.Hour;
    if (hora < 8 || hora >= 18)
    {
        motivo = "Transacao fora do horario comercial.";
        return false;
    }
    return true;
}
```

Como verificar:
- adicionar a validacao ao pipeline
- executar fora do intervalo e observar a rejeicao com motivo

2. Implementacao de referencia:

```csharp
Func<IConta, string> formatoTexto = conta => conta.ExibirExtrato();
Func<IConta, string> formatoCsv = conta => $"{conta.Numero};{conta.Titular.Nome};{conta.Saldo}";
```

Como verificar:
- a mesma conta produz duas strings com formatos diferentes

3. Implementacao de referencia:

```csharp
public bool SacarComConfirmacao(decimal valor, Func<decimal, bool> confirmar)
{
    if (!confirmar(valor)) return false;
    return Sacar(valor);
}
```

Solucao minima: chamar o callback e, se retornar `true`, executar o saque.
Solucao mais idiomatica: injetar o callback no fluxo apenas quando a regra de confirmacao fizer sentido para aquele caso de uso.

Erros comuns:
- usar `Action` quando e preciso retorno booleano
- misturar validacao com efeito colateral no mesmo delegate sem necessidade
- reapresentar o mesmo modelo de evento da Aula 6 sem explicar o tradeoff

## Fechamento e conexao com a proxima aula

Com delegates, o aluno passa a enxergar melhor o mecanismo que estava por tras dos eventos. A Aula 9 volta o foco para arquitetura: interfaces, DI, composicao raiz e testabilidade.

### Versao esperada apos esta aula

- Versao de entrada: `v0.6`
- Versao de saida: `v0.7`
- Classes novas: validadores, central de notificacoes e componente com `Func`
- Classes alteradas: `ContaBase`
- Comportamentos novos: pipeline de validacao, callbacks, notificacoes por handlers registrados
- Como testar no Main: executar uma operacao aprovada e outra rejeitada pelo pipeline e comparar as saidas
