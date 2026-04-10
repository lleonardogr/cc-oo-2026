using MiniBank.Contracts;
using MiniBank.Models.Transacoes;
using MiniBank.Strategies;

namespace MiniBank.Services;

public class ServicoTransferencia
{
    private readonly ICalculadoraTaxa calculadoraTaxa;

    public ServicoTransferencia(ICalculadoraTaxa calculadoraTaxa)
    {
        this.calculadoraTaxa = calculadoraTaxa;
    }

    public bool Executar(IConta origem, IConta destino, decimal valor)
    {
        if (origem.Numero == destino.Numero)
        {
            throw new InvalidOperationException("Nao pode transferir para a mesma conta.");
        }

        if (valor <= 0)
        {
            throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        }

        var taxa = calculadoraTaxa.Calcular(valor);
        var total = valor + taxa;

        origem.Sacar(total);
        destino.Depositar(valor);

        if (origem is Models.Contas.ContaBase contaBase)
        {
            contaBase.Extrato.Registrar(new Transacao(valor, TipoTransacao.Transferencia, $"Transferencia para {destino.Numero}"));
            if (taxa > 0)
            {
                contaBase.Extrato.Registrar(new Transacao(taxa, TipoTransacao.Taxa, "Taxa de transferencia"));
            }
        }

        return true;
    }
}
