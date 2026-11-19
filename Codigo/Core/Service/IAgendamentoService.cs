using System;
using System.Collections.Generic;

namespace Core.Service;

public interface IAgendamentoService
{
    IEnumerable<Agendamento> GetByData(
        DateTime data,
        uint idEstabelecimento,
        uint? idProfissional = null);

    Agendamento? Get(uint id, uint idEstabelecimento);

    uint Create(Agendamento agendamento, IEnumerable<uint> idsServicos);

    void Cancel(uint id, uint idEstabelecimento);

    void ConfirmarSinal(uint id, uint idEstabelecimento);
}
