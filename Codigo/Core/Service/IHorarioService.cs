namespace Core.Service;

public interface IHorarioService
{
    uint Create(Horario horario);
    void Edit(Horario horario, uint idEstabelecimento);
    void Delete(uint id, uint idEstabelecimento);
    Horario? Get(uint id, uint idEstabelecimento);
    IEnumerable<Horario> GetAll(uint idEstabelecimento);
    IEnumerable<Horario> GetByProfissional(uint idProfissional, uint idEstabelecimento);
    IEnumerable<Horario> GetByDiaSemana(string diaSemana, uint idProfissional, uint idEstabelecimento);
}
