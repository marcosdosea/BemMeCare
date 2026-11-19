namespace Core.Service;

public interface IAgendamentoService
{
    uint Create(Agendamento agendamento);
    void Edit(Agendamento agendamento);
    void Delete(uint id, uint idEstabelecimento);
    Agendamento? Get(uint id, uint idEstabelecimento);
    IEnumerable<Agendamento> GetAll(uint idEstabelecimento);
}
