using MiniBank.Repositories.InMemory;
using MiniBank.Services;
using MiniBank.Strategies;
using MiniBank.UI.Console;

var repoClientes = new RepositorioClienteEmMemoria();
var repoContas = new RepositorioContaEmMemoria();
var calculadoraTaxa = new TaxaContaCorrente();

var servico = new ServicoBancario(repoClientes, repoContas, calculadoraTaxa);

PopularDadosIniciais(servico);

var app = new MiniBankConsoleApp(servico, repoClientes, repoContas);
app.Executar();

static void PopularDadosIniciais(ServicoBancario servico)
{
    var ana = servico.CadastrarCliente("Ana Silva", "123.456.789-00", "ana@email.com");
    ana.Apelido = "Aninha";
    ana.Telefone = "(11) 99999-0000";

    var joao = servico.CadastrarCliente("Joao Santos", "987.654.321-00", "joao@email.com");

    var ccAna = servico.AbrirContaCorrente(ana, 5_000m);
    var cpAna = servico.AbrirContaPoupanca(ana, 8_000m);
    var ccJoao = servico.AbrirContaCorrente(joao, 1_000m);

    servico.Depositar(ccAna.Numero, 2_000m);
    servico.Transferir(ccAna.Numero, ccJoao.Numero, 1_000m);
    cpAna.AplicarRendimento();
}
