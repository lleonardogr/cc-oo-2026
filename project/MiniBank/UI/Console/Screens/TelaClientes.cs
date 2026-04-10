using MiniBank.Repositories.Contracts;
using Spectre.Console;

namespace MiniBank.UI.Console.Screens;

public class TelaClientes
{
    public void Exibir(IRepositorioCliente repositorioClientes)
    {
        var clientes = repositorioClientes.ListarTodos().ToList();

        if (clientes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nenhum cliente cadastrado.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Clientes cadastrados[/]");

        table.AddColumn("Nome");
        table.AddColumn("CPF");
        table.AddColumn("Email");
        table.AddColumn("Resumo");

        foreach (var cliente in clientes)
        {
            table.AddRow(
                cliente.Nome,
                cliente.Cpf,
                cliente.Email,
                cliente.ExibirResumo());
        }

        AnsiConsole.Write(table);
    }
}
