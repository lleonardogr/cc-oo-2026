using MiniBank.Repositories.Contracts;
using Spectre.Console;

namespace MiniBank.UI.Console.Screens;

public class TelaContas
{
    public void Exibir(IRepositorioConta repositorioContas)
    {
        var contas = repositorioContas.ListarTodas().ToList();

        if (contas.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nenhuma conta cadastrada.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Contas cadastradas[/]");

        table.AddColumn("Numero");
        table.AddColumn("Titular");
        table.AddColumn("Tipo");
        table.AddColumn("Saldo");
        table.AddColumn("Status");

        foreach (var conta in contas)
        {
            var saldoFormatado = conta.Saldo switch
            {
                < 100m => $"[red]{conta.Saldo:C}[/]",
                _ => $"[green]{conta.Saldo:C}[/]"
            };

            table.AddRow(
                conta.Numero,
                conta.Titular.Nome,
                conta.GetType().Name,
                saldoFormatado,
                conta.Ativa ? "[green]Ativa[/]" : "[red]Inativa[/]");
        }

        AnsiConsole.Write(table);
    }
}
