# Aula de Orientacao a Objetos com C#

Material didatico em Markdown baseado no EPUB `Programacao Orientada a Objetos com C 2019.epub` (`Hands-On Object-Oriented Programming with C#`, Raihan Taher, Packt 2019).

## 🏦 Projeto Progressivo: MiniBank

As aulas sao conectadas por um **projeto bancario** construido incrementalmente. Cada aula aplica o conceito aprendido para evoluir a aplicacao:

| Aula | Versao | O que muda no MiniBank |
|------|--------|----------------------|
| 0 | v0.0 | Rascunho com campos publicos |
| 1 | v0.1 | Construtores, propriedades, validacao basica |
| 2 | v0.2 | Heranca (ContaCorrente/Poupanca), interface IConta, polimorfismo |
| 3 | v0.3 | Interfaces segregadas, sealed, propriedades robustas |
| 4 | v0.4 | Banco (agregacao), Transacao/Extrato (composicao), transferencia |
| 5 | — | Diagramas UML do sistema completo |
| 6 | v0.5 | Excecoes customizadas, eventos de transacao, repositorio generico |
| 7 | v0.6 | SOLID, Strategy para taxas, SRP com ServicoTransferencia |
| 8 | v0.7 | Pipeline de validacao com delegates, central de notificacoes |
| 9 | v0.8 | DI com interfaces de repositorio, testes sem banco de dados |
| 10 | v0.9 | Repositorio generico unificado, covariancia, metodos utilitarios |
| 11 | v1.0 | Records, nullable refs, pattern matching, default interface methods |
| 12 | v1.1 | Interface de console com Spectre.Console, menu interativo, tabelas e paineis |

## Trilha de aulas

### Fundamentos (Aulas 0–4)

1. [00-introducao-a-poo.md](./00-introducao-a-poo.md) — paradigma OO, objetos, primeiro rascunho do MiniBank
2. [01-classes-e-objetos.md](./01-classes-e-objetos.md) — classes, construtores, validacao, propriedades
3. [02-pilares-da-poo.md](./02-pilares-da-poo.md) — encapsulamento, heranca, abstracao, polimorfismo
4. [03-recursos-de-poo-em-csharp.md](./03-recursos-de-poo-em-csharp.md) — interfaces, abstract, sealed, partial, modificadores
5. [04-colaboracao-entre-objetos.md](./04-colaboracao-entre-objetos.md) — dependencia, associacao, agregacao, composicao

### Modelagem (Aula 5)

6. [05-modelagem-e-diagramas.md](./05-modelagem-e-diagramas.md) — UML: classes, casos de uso, sequencia

### Robustez e Desacoplamento (Aulas 6–8)

7. [06-excecoes-eventos-e-generics.md](./06-excecoes-eventos-e-generics.md) — excecoes customizadas, eventos, repositorio generico
8. [07-solid-e-padroes.md](./07-solid-e-padroes.md) — SOLID, Strategy, refatoracao
9. [08-delegates-e-eventos.md](./08-delegates-e-eventos.md) — delegates, multicasting, lambdas, pipeline de validacao

### Arquitetura e Reuso (Aulas 9–10)

10. [09-interfaces-avancadas.md](./09-interfaces-avancadas.md) — DI, testabilidade, composicao raiz
11. [10-generics-em-profundidade.md](./10-generics-em-profundidade.md) — constraints, covariancia, repositorio unificado

### C# Moderno (Aula 11)

12. [11-novidades-csharp-e-poo.md](./11-novidades-csharp-e-poo.md) — records, nullable, pattern matching, versao final v1.0

### Interface e Produto (Aula 12)

13. [12-interface-com-spectre-console.md](./12-interface-com-spectre-console.md) — interface de console com Spectre.Console para o MiniBank v1.1

## Observacao

Os exemplos conceituais (Animal, Funcionario, etc.) foram mantidos para ilustrar teoria. O projeto MiniBank aparece em secoes **🏦 Hands-on** em cada aula, dando progressao e contexto pratico continuo.
