using MiniBank.Contracts;
using MiniBank.Models;
using MiniBank.Models.Contas;
using MiniBank.Repositories.Contracts;
using MiniBank.Strategies;

namespace MiniBank.Services;

public class ServicoBancario
{
    private readonly IRepositorioCliente repoClientes;
    private readonly IRepositorioConta repoContas;
    private readonly ServicoTransferencia servicoTransferencia;
    private int contadorContas;

    public ServicoBancario(
        IRepositorioCliente repoClientes,
        IRepositorioConta repoContas,
        ICalculadoraTaxa calculadoraTaxa)
    {
        this.repoClientes = repoClientes;
        this.repoContas = repoContas;
        servicoTransferencia = new ServicoTransferencia(calculadoraTaxa);
    }

    public Cliente CadastrarCliente(string nome, string cpf, string email)
    {
        if (repoClientes.BuscarPorCpf(cpf) is not null)
        {
            throw new InvalidOperationException("Cliente ja cadastrado.");
        }

        var cliente = new Cliente(nome, cpf, email);
        repoClientes.Salvar(cliente);
        return cliente;
    }

    public ContaCorrente AbrirContaCorrente(Cliente cliente, decimal saldoInicial = 0m)
    {
        var conta = new ContaCorrente($"CC-{++contadorContas:D4}", cliente, saldoInicial);
        repoContas.Salvar(conta);
        return conta;
    }

    public ContaPoupanca AbrirContaPoupanca(Cliente cliente, decimal saldoInicial = 0m)
    {
        var conta = new ContaPoupanca($"CP-{++contadorContas:D4}", cliente, saldoInicial);
        repoContas.Salvar(conta);
        return conta;
    }

    public void Depositar(string numeroConta, decimal valor)
    {
        var conta = BuscarContaOuFalhar(numeroConta);
        conta.Depositar(valor);
        repoContas.Salvar(conta);
    }

    public void Sacar(string numeroConta, decimal valor)
    {
        var conta = BuscarContaOuFalhar(numeroConta);
        conta.Sacar(valor);
        repoContas.Salvar(conta);
    }

    public bool Transferir(string numeroOrigem, string numeroDestino, decimal valor)
    {
        var origem = BuscarContaOuFalhar(numeroOrigem);
        var destino = BuscarContaOuFalhar(numeroDestino);

        var ok = servicoTransferencia.Executar(origem, destino, valor);
        repoContas.Salvar(origem);
        repoContas.Salvar(destino);
        return ok;
    }

    public IEnumerable<IConta> ListarContas()
        => repoContas.ListarTodas();

    public IEnumerable<Cliente> ListarClientes()
        => repoClientes.ListarTodos();

    public IConta? BuscarConta(string numero)
        => repoContas.BuscarPorNumero(numero);

    private IConta BuscarContaOuFalhar(string numero)
        => repoContas.BuscarPorNumero(numero)
            ?? throw new InvalidOperationException($"Conta {numero} nao encontrada.");
}
