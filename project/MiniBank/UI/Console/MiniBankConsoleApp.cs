using MiniBank.Repositories.Contracts;
using MiniBank.Services;
using MiniBank.UI.Console.Screens;
using Spectre.Console;

namespace MiniBank.UI.Console;

public class MiniBankConsoleApp
{
    private readonly ServicoBancario servico;
    private readonly IRepositorioCliente repoClientes;
    private readonly IRepositorioConta repoContas;
    private readonly MenuPrincipal menu = new();
    private readonly TelaClientes telaClientes = new();
    private readonly TelaContas telaContas = new();

    public MiniBankConsoleApp(
        ServicoBancario servico,
        IRepositorioCliente repoClientes,
        IRepositorioConta repoContas)
    {
        this.servico = servico;
        this.repoClientes = repoClientes;
        this.repoContas = repoContas;
    }

    public void Executar()
    {
        var executando = true;

        while (executando)
        {
            var opcao = menu.Exibir();

            try
            {
                switch (opcao)
                {
                    case MenuOpcao.Dashboard:
                        ExibirDashboard();
                        break;
                    case MenuOpcao.CadastrarCliente:
                        CadastrarCliente();
                        break;
                    case MenuOpcao.AbrirContaCorrente:
                        AbrirContaCorrente();
                        break;
                    case MenuOpcao.AbrirContaPoupanca:
                        AbrirContaPoupanca();
                        break;
                    case MenuOpcao.Depositar:
                        Depositar();
                        break;
                    case MenuOpcao.Sacar:
                        Sacar();
                        break;
                    case MenuOpcao.Transferir:
                        Transferir();
                        break;
                    case MenuOpcao.ListarClientes:
                        telaClientes.Exibir(repoClientes);
                        break;
                    case MenuOpcao.ListarContas:
                        telaContas.Exibir(repoContas);
                        break;
                    case MenuOpcao.VerExtrato:
                        VerExtrato();
                        break;
                    case MenuOpcao.Sair:
                        executando = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Write(
                    new Panel(new Text(ex.Message))
                        .Header("[red]Erro[/]")
                        .Border(BoxBorder.Rounded));
            }

            if (executando)
            {
                Pausar();
            }
        }
    }

    private void ExibirDashboard()
    {
        var clientes = repoClientes.ListarTodos().ToList();
        var contas = repoContas.ListarTodas().ToList();
        var maiorSaldo = contas.Count == 0 ? 0m : contas.Max(c => c.Saldo);

        var conteudo =
            $"Clientes: [yellow]{clientes.Count}[/]{Environment.NewLine}" +
            $"Contas: [yellow]{contas.Count}[/]{Environment.NewLine}" +
            $"Maior saldo: [green]{maiorSaldo:C}[/]";

        AnsiConsole.Write(
            new Panel(conteudo)
                .Header("[blue]Dashboard[/]")
                .Border(BoxBorder.Double));
    }

    private void CadastrarCliente()
    {
        var nome = AnsiConsole.Prompt(
            new TextPrompt<string>("Nome do [green]cliente[/]:")
                .Validate(n => string.IsNullOrWhiteSpace(n)
                    ? ValidationResult.Error("[red]Nome obrigatorio[/]")
                    : ValidationResult.Success()));

        var cpf = AnsiConsole.Prompt(
            new TextPrompt<string>("CPF ([grey]XXX.XXX.XXX-XX[/]):")
                .Validate(c => c.Length == 14
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Use o formato XXX.XXX.XXX-XX[/]")));

        var email = AnsiConsole.Prompt(
            new TextPrompt<string>("Email:")
                .Validate(e => e.Contains('@')
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Email invalido[/]")));

        var cliente = servico.CadastrarCliente(nome, cpf, email);
        AnsiConsole.MarkupLine($"[green]Cliente cadastrado:[/] {Markup.Escape(cliente.ToString())}");
    }

    private void AbrirContaCorrente()
    {
        var cliente = SelecionarClientePorCpf();
        var saldoInicial = PerguntarValor("Saldo inicial:");

        var conta = servico.AbrirContaCorrente(cliente, saldoInicial);
        AnsiConsole.MarkupLine($"[green]Conta corrente criada:[/] [yellow]{conta.Numero}[/]");
    }

    private void AbrirContaPoupanca()
    {
        var cliente = SelecionarClientePorCpf();
        var saldoInicial = PerguntarValor("Saldo inicial:");

        var conta = servico.AbrirContaPoupanca(cliente, saldoInicial);
        AnsiConsole.MarkupLine($"[green]Conta poupanca criada:[/] [yellow]{conta.Numero}[/]");
    }

    private void Depositar()
    {
        var numero = PerguntarNumeroConta("Numero da conta para deposito:");
        var valor = PerguntarValor("Valor do deposito:");

        servico.Depositar(numero, valor);
        var conta = repoContas.BuscarPorNumero(numero)!;
        AnsiConsole.MarkupLine($"[green]Deposito realizado.[/] Novo saldo: [yellow]{conta.Saldo:C}[/]");
    }

    private void Sacar()
    {
        var numero = PerguntarNumeroConta("Numero da conta para saque:");
        var valor = PerguntarValor("Valor do saque:");

        servico.Sacar(numero, valor);
        var conta = repoContas.BuscarPorNumero(numero)!;
        AnsiConsole.MarkupLine($"[green]Saque realizado.[/] Novo saldo: [yellow]{conta.Saldo:C}[/]");
    }

    private void Transferir()
    {
        var origem = PerguntarNumeroConta("Conta de origem:");
        var destino = PerguntarNumeroConta("Conta de destino:");
        var valor = PerguntarValor("Valor da transferencia:");

        if (!AnsiConsole.Confirm("Confirma a transferencia?"))
        {
            AnsiConsole.MarkupLine("[yellow]Transferencia cancelada.[/]");
            return;
        }

        servico.Transferir(origem, destino, valor);
        AnsiConsole.MarkupLine("[green]Transferencia realizada com sucesso.[/]");
    }

    private void VerExtrato()
    {
        var numero = PerguntarNumeroConta("Numero da conta:");
        var conta = repoContas.BuscarPorNumero(numero);

        if (conta is null)
        {
            AnsiConsole.MarkupLine("[red]Conta nao encontrada.[/]");
            return;
        }

        AnsiConsole.Write(
            new Panel(new Text(conta.ExibirExtrato()))
                .Header($"Extrato da conta {conta.Numero}")
                .Border(BoxBorder.Double));
    }

    private MiniBank.Models.Cliente SelecionarClientePorCpf()
    {
        if (!repoClientes.ListarTodos().Any())
        {
            throw new InvalidOperationException("Cadastre um cliente antes de abrir contas.");
        }

        var cpf = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Selecione o [green]CPF[/] do cliente:")
                .PageSize(10)
                .AddChoices(repoClientes.ListarTodos().Select(c => c.Cpf)));

        return repoClientes.BuscarPorCpf(cpf)
            ?? throw new InvalidOperationException("Cliente nao encontrado.");
    }

    private static string PerguntarNumeroConta(string mensagem)
        => AnsiConsole.Prompt(
            new TextPrompt<string>(mensagem)
                .Validate(n => string.IsNullOrWhiteSpace(n)
                    ? ValidationResult.Error("[red]Numero obrigatorio[/]")
                    : ValidationResult.Success()));

    private static decimal PerguntarValor(string mensagem)
        => AnsiConsole.Prompt(
            new TextPrompt<decimal>(mensagem)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Informe um valor positivo[/]")));

    private static void Pausar()
    {
        AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para continuar...[/]");
        global::System.Console.ReadKey(true);
    }
}
