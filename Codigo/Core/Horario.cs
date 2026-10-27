using System;
using System.Collections.Generic;

namespace Core;

public partial class Horario
{
    public uint Id { get; set; }

    public TimeSpan InicioTurno { get; set; }

    public TimeSpan FimTurno { get; set; }

    public DateTime InicioVigencia { get; set; }

    public DateTime? FimVigencia { get; set; }

    public string DiaSemana { get; set; } = null!;

    public uint IdProfissional { get; set; }

    public uint IdEstabelecimento { get; set; }

    public virtual Profissional Profissional { get; set; } = null!;
}
