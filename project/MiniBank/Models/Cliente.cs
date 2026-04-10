using System.Text.RegularExpressions;
using MiniBank.Repositories.Contracts;

namespace MiniBank.Models;

public class Cliente : IIdentificavel
{
    private static readonly Regex CpfRegex = new(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", RegexOptions.Compiled);

    public string Id => Cpf;
    public string Nome { get; }
    public string Cpf { get; }
    public string Email { get; private set; }
    public string? Telefone { get; set; }
    public string? Apelido { get; set; }

    public Cliente(string nome, string cpf, string email)
    {
        Nome = !string.IsNullOrWhiteSpace(nome) ? nome : throw new ArgumentException("Nome obrigatorio.", nameof(nome));
        Cpf = CpfRegex.IsMatch(cpf) ? cpf : throw new ArgumentException("CPF deve estar no formato XXX.XXX.XXX-XX.", nameof(cpf));
        Email = email.Contains('@') ? email : throw new ArgumentException("Email invalido.", nameof(email));
    }

    public void AtualizarEmail(string email)
    {
        Email = email.Contains('@') ? email : throw new ArgumentException("Email invalido.", nameof(email));
    }

    public string ExibirResumo()
    {
        var apelido = Apelido is null ? string.Empty : $" ({Apelido})";
        var telefone = Telefone ?? "nao informado";
        return $"{Nome}{apelido} | Tel: {telefone}";
    }

    public override string ToString()
        => $"{Nome} (CPF: {Cpf})";
}
