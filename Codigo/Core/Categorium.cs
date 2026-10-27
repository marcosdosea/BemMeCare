using System;
using System.Collections.Generic;

namespace Core;

public partial class Categorium
{
    public uint Id { get; set; }

    public string Nome { get; set; } = null!;

    public uint IdEstabelecimento { get; set; }

    public virtual Estabelecimento IdEstabelecimentoNavigation { get; set; } = null!;

    public virtual ICollection<Servico> Servicos { get; set; } = new List<Servico>();

    public virtual ICollection<Profissional> Profissionals { get; set; } = new List<Profissional>();
}
