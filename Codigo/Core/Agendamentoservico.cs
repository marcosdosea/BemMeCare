using System;
using System.Collections.Generic;

namespace Core;

public partial class Agendamentoservico
{
    public uint IdServico { get; set; }

    public uint IdAgendamento { get; set; }

    public decimal PrecoCobrado { get; set; }

    public virtual Agendamento IdAgendamentoNavigation { get; set; } = null!;

    public virtual Servico IdServicoNavigation { get; set; } = null!;
}
