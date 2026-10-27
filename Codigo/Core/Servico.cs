using System;
using System.Collections.Generic;

namespace Core;

public partial class Servico
{
    public uint Id { get; set; }

    public string Nome { get; set; } = null!;

    public uint IdCategoria { get; set; }

    public uint IdEstabelecimento { get; set; }

    public virtual ICollection<Agendamentoservico> Agendamentoservicos { get; set; } = new List<Agendamentoservico>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Estabelecimento IdEstabelecimentoNavigation { get; set; } = null!;

    public virtual ICollection<Servicoprofissional> Servicoprofissionals { get; set; } = new List<Servicoprofissional>();
}
