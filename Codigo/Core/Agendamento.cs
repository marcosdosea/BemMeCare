using System;
using System.Collections.Generic;

namespace Core;

public partial class Agendamento
{
    public uint Id { get; set; }

    public decimal PrecoFinal { get; set; }

    public DateTime DataHoraAtendimento { get; set; }

    public string? Observacoes { get; set; }

    public uint DuracaoMinutos { get; set; }

    /// <summary>
    /// PE - PENDENTE
    /// CA - CANCELADO
    /// CO - COMPLETO
    /// SP - SINAL PAGO
    /// </summary>
    public string StatusAgendamento { get; set; } = null!;

    public DateTime DataHoraCriacao { get; set; }

    /// <summary>
    /// MA - MARCADO
    /// CA - CANCELADO
    /// FI - FINALIZADO
    /// </summary>
    public string StatusPagamento { get; set; } = null!;

    public decimal ValorPagoSinal { get; set; }

    public DateTime DataHoraFimAtendimento { get; set; }

    public uint IdPessoaCliente { get; set; }

    public uint IdProfissional { get; set; }

    public uint IdEstabelecimento { get; set; }

    public uint IdPessoaMarcou { get; set; }

    public virtual ICollection<Agendamentoservico> Agendamentoservicos { get; set; } = new List<Agendamentoservico>();

    public virtual Pessoa IdPessoaClienteNavigation { get; set; } = null!;

    public virtual Pessoa IdPessoaMarcouNavigation { get; set; } = null!;

    public virtual Profissional Profissional { get; set; } = null!;
}
