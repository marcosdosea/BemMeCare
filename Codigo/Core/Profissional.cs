using System;
using System.Collections.Generic;

namespace Core;

public partial class Profissional
{
    public uint IdPessoa { get; set; }

    public uint IdEstabelecimento { get; set; }

    public sbyte GerirHorario { get; set; }

    public sbyte EstaAtivo { get; set; }

    public sbyte GerirServico { get; set; }

    public sbyte ReceberPagamentoDireto { get; set; }

    public string? ChavePix { get; set; }

    public uint DuracaoIntervaloTempo { get; set; }

    public sbyte? EProfissional { get; set; }

    public sbyte? EAtendente { get; set; }

    public virtual ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual Estabelecimento IdEstabelecimentoNavigation { get; set; } = null!;

    public virtual Pessoa IdPessoaNavigation { get; set; } = null!;

    public virtual ICollection<Servicoprofissional> Servicoprofissionals { get; set; } = new List<Servicoprofissional>();

    public virtual ICollection<Categorium> IdCategoria { get; set; } = new List<Categorium>();
}
