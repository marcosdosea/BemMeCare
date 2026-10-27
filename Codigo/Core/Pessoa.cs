using System;
using System.Collections.Generic;

namespace Core;

public partial class Pessoa
{
    public uint Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public string Telefone { get; set; } = null!;

    public DateTime DataDeNascimento { get; set; }

    public string Cor { get; set; } = null!;

    public string Genero { get; set; } = null!;

    public string Sexualidade { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public string Cep { get; set; } = null!;

    public string Rua { get; set; } = null!;

    public string Bairro { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Complemento { get; set; } = null!;

    public string Cidade { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<Agendamento> AgendamentoIdPessoaClienteNavigations { get; set; } = new List<Agendamento>();

    public virtual ICollection<Agendamento> AgendamentoIdPessoaMarcouNavigations { get; set; } = new List<Agendamento>();

    public virtual ICollection<Estabelecimento> Estabelecimentos { get; set; } = new List<Estabelecimento>();

    public virtual ICollection<Profissional> Profissionals { get; set; } = new List<Profissional>();
}
