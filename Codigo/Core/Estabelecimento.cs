using System;
using System.Collections.Generic;

namespace Core;

public partial class Estabelecimento
{
    public uint Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Modalidade { get; set; } = null!;

    public string Cnpj { get; set; } = null!;

    public string Cep { get; set; } = null!;

    public string Rua { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = null!;

    public string Cidade { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string Contato { get; set; } = null!;

    public string ChavePix { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public uint IdPessoa { get; set; }

    public virtual ICollection<Categorium> Categoria { get; set; } = new List<Categorium>();

    public virtual Pessoa IdPessoaNavigation { get; set; } = null!;

    public virtual ICollection<Profissional> Profissionals { get; set; } = new List<Profissional>();

    public virtual ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}
