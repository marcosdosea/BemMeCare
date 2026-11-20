using System;
using System.Collections.Generic;
using Core.DTO;

namespace Core.Service;

public interface IAgendamentoDeClientesService
{
    IEnumerable<HorarioDisponivelDTO> GetHorariosDisponiveis(
        uint idProfissional,
        uint idServico,
        DateTime data,
        uint idEstabelecimento);

    AgendamentoDTO CriarResumo(
        uint idProfissional,
        uint idServico,
        DateTime dataHoraAtendimento,
        uint idEstabelecimento);

    IEnumerable<Agendamento> GetByPeriodo(
        DateTime dataInicio,
        DateTime dataFimExclusiva,
        uint idEstabelecimento,
        uint? idProfissional = null);

    IEnumerable<Agendamento> GetByData(
        DateTime data,
        uint idEstabelecimento,
        uint? idProfissional = null);

    Agendamento? Get(uint id, uint idEstabelecimento);

    uint Create(Agendamento agendamento, IEnumerable<uint> idsServicos);

    void Cancel(uint id, uint idEstabelecimento);

    void ConfirmarSinal(uint id, uint idEstabelecimento);
}
