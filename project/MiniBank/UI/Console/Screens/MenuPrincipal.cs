using Spectre.Console;

namespace MiniBank.UI.Console.Screens;

public class MenuPrincipal
{
    public MenuOpcao Exibir()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new Panel("[bold yellow]MiniBank[/]\n[grey]Sistema bancario em console[/]")
                .Border(BoxBorder.Rounded)
                .Header("[blue]Menu Principal[/]")
                .Expand());

        return AnsiConsole.Prompt(
            new SelectionPrompt<MenuOpcao>()
                .Title("\n[green]Escolha uma operacao[/]")
                .UseConverter(Converter)
                .PageSize(12)
                .AddChoices(
                    MenuOpcao.Dashboard,
                    MenuOpcao.CadastrarCliente,
                    MenuOpcao.AbrirContaCorrente,
                    MenuOpcao.AbrirContaPoupanca,
                    MenuOpcao.Depositar,
                    MenuOpcao.Sacar,
                    MenuOpcao.Transferir,
                    MenuOpcao.ListarClientes,
                    MenuOpcao.ListarContas,
                    MenuOpcao.VerExtrato,
                    MenuOpcao.Sair));
    }

    private static string Converter(MenuOpcao opcao)
        => opcao switch
        {
            MenuOpcao.Dashboard => "Dashboard",
            MenuOpcao.CadastrarCliente => "Cadastrar cliente",
            MenuOpcao.AbrirContaCorrente => "Abrir conta corrente",
            MenuOpcao.AbrirContaPoupanca => "Abrir conta poupanca",
            MenuOpcao.Depositar => "Depositar",
            MenuOpcao.Sacar => "Sacar",
            MenuOpcao.Transferir => "Transferir",
            MenuOpcao.ListarClientes => "Listar clientes",
            MenuOpcao.ListarContas => "Listar contas",
            MenuOpcao.VerExtrato => "Ver extrato",
            MenuOpcao.Sair => "Sair",
            _ => opcao.ToString()
        };
}
