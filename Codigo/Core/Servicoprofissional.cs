using System;
using System.Collections.Generic;

namespace Core;

public partial class Servicoprofissional
{
    public uint IdProfissional { get; set; }

    public uint IdEstabelecimento { get; set; }

    public uint IdServico { get; set; }

    public sbyte ExigeAnamnese { get; set; }

    public string FormAnamnese { get; set; } = null!;

    public decimal Preco { get; set; }

    public float PercentualSinal { get; set; }

    public string QuandoPagarSinal { get; set; } = null!;

    public uint Duracao { get; set; }

    public virtual Servico IdServicoNavigation { get; set; } = null!;

    public virtual Profissional Profissional { get; set; } = null!;
}
