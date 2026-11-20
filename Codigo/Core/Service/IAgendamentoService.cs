using Core.DTO;

namespace Core.Service;

public interface IAgendamentoService
{
    uint Create(SelecaoAgendamentoDTO selecao, uint idEstabelecimento, uint idPessoa);
    bool Cancel(uint id, uint idEstabelecimento);
    IEnumerable<AgendamentoDTO> GetAll(uint idPessoa, uint idEstabelecimento);
    bool ValidateProfissionalServicosAgendamento(SelecaoAgendamentoDTO selecao, uint idEstabelecimento);
    IEnumerable<TimeOnly> GetHorariosDisponiveisProfissionalAgendamento(SelecaoAgendamentoDTO selecao, IEnumerable<Horario> horarios, uint idEstabelecimento);
    AgendamentoResumoDTO GetResumoAgendamento(SelecaoAgendamentoDTO selecao, uint idEstabelecimento);
}
