# Aula 5 - Modelagem e Diagramas

## Teoria

Modelar antes de codificar diminui retrabalho. A **UML** (Unified Modeling Language) e o padrao para modelagem de software. Diagramas UML se dividem em estruturais (classes, componentes) e comportamentais (sequencia, caso de uso).

Para uma aula introdutoria, tres visoes sao essenciais:

- **Diagrama de classes**: estrutura estatica
- **Caso de uso**: quem interage com o sistema e para que
- **Diagrama de sequencia**: ordem temporal das mensagens

### Elementos do diagrama de classes

Cada classe e um retangulo com tres compartimentos: nome, atributos, metodos. Simbolos: `+` public, `-` private, `#` protected. Relacoes: heranca (seta com triangulo), associacao (seta), agregacao (losango vazio), composicao (losango cheio), dependencia (seta tracejada).

### Elementos do diagrama de sequencia

Lifelines verticais representam objetos. Mensagens fluem horizontalmente, de cima para baixo (tempo). Setas solidas = chamada sincrona. Setas tracejadas = retorno.

---

## 🏦 Hands-on: App Bancario — Diagramas do MiniBank

Ja construimos bastante codigo. Agora vamos **modelar o que temos** e **planejar o que falta**.

### Diagrama de classes completo (v0.4)

```mermaid
classDiagram
    class Banco {
        +string Nome
        -List~Cliente~ clientes
        -List~IConta~ contas
        +AdicionarCliente(Cliente)
        +AbrirContaCorrente(Cliente, decimal) ContaCorrente
        +AbrirContaPoupanca(Cliente, decimal) ContaPoupanca
        +ListarContas() IReadOnlyList~IConta~
    }

    class Cliente {
        +string Nome
        +string Cpf
        +string Email
    }

    class IConta {
        <<interface>>
        +string Numero
        +decimal Saldo
        +Cliente Titular
        +Depositar(decimal)
        +Sacar(decimal) bool
        +ExibirExtrato() string
    }

    class ContaBase {
        <<abstract>>
        #decimal Saldo
        +Extrato Extrato
        +Depositar(decimal)
        +Sacar(decimal)* bool
    }

    class ContaCorrente {
        +decimal LimiteChequeEspecial
        +Transferir(IConta, decimal) bool
    }

    class ContaPoupanca {
        +decimal TaxaRendimento
        +AplicarRendimento()
    }

    class Extrato {
        -List~Transacao~ transacoes
        +Registrar(Transacao)
        +Imprimir()
    }

    class Transacao {
        +decimal Valor
        +DateTime Data
        +TipoTransacao Tipo
        +string Descricao
    }

    class EmissorExtrato {
        +EmitirParaConsole(IConta)
    }

    Banco o-- Cliente : agrega
    Banco o-- IConta : agrega
    IConta <|.. ContaBase
    ContaBase <|-- ContaCorrente
    ContaBase <|-- ContaPoupanca
    ContaBase --> Cliente : associacao
    ContaBase *-- Extrato : composicao
    Extrato *-- Transacao : composicao
    EmissorExtrato ..> IConta : depende
```

### Caso de uso

```mermaid
flowchart LR
    Cli([Cliente]) --> UC1[Abrir conta]
    Cli --> UC2[Depositar]
    Cli --> UC3[Sacar]
    Cli --> UC4[Transferir]
    Cli --> UC5[Consultar extrato]
    Gerente([Gerente]) --> UC6[Cadastrar cliente]
    Gerente --> UC7[Gerar relatorio]
```

### Diagrama de sequencia — Transferencia

```mermaid
sequenceDiagram
    participant C as Cliente (usuario)
    participant B as Banco
    participant O as ContaOrigem
    participant D as ContaDestino
    participant E as Extrato

    C->>B: Solicita transferencia
    B->>O: Sacar(valor)
    alt Saldo suficiente
        O->>E: Registrar(Saque)
        O-->>B: true
        B->>D: Depositar(valor)
        D->>E: Registrar(Deposito)
        B-->>C: Transferencia confirmada
    else Saldo insuficiente
        O-->>B: false
        B-->>C: Transferencia negada
    end
```

### Diagrama de sequencia — Abrir conta

```mermaid
sequenceDiagram
    participant G as Gerente
    participant B as Banco
    participant Cli as Cliente
    participant CC as ContaCorrente

    G->>B: AbrirContaCorrente(cliente, 1000)
    B->>CC: new ContaCorrente("CC-001", cliente, 1000)
    CC->>CC: Registrar saldo inicial no Extrato
    CC-->>B: conta criada
    B-->>G: retorna ContaCorrente
```

### Do diagrama ao codigo: o que falta?

Olhando o diagrama de caso de uso, identificamos funcionalidades que ainda nao implementamos:

| Caso de uso | Status | Aula prevista |
|-------------|--------|---------------|
| Abrir conta | ✅ Implementado | Aula 4 |
| Depositar | ✅ Implementado | Aula 1 |
| Sacar | ✅ Implementado | Aula 2 |
| Transferir | ✅ Implementado | Aula 4 |
| Consultar extrato | ✅ Implementado | Aula 4 |
| Notificar transacao | ❌ Pendente | Aula 6 (eventos) |
| Persistir dados | ❌ Pendente | Aula 9 (interfaces + DI) |
| Aplicar taxas variadas | ❌ Pendente | Aula 7 (Strategy) |

---

## Exercicios

1. Desenhe em Mermaid um diagrama de sequencia para "Cliente aplica rendimento na poupanca e consulta extrato".
2. Adicione ao diagrama de classes uma classe `Relatorio` com metodo `GerarResumo(Banco)` e indique o tipo de relacao com `Banco`.
3. Identifique no diagrama de classes pelo menos uma violacao potencial do SRP. Qual classe faz coisas demais?
