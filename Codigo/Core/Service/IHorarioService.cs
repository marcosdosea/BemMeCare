namespace Core.Service;

public interface IHorarioService
{
    uint Create(Horario horario);
    void Edit(Horario horario);
    void Delete(uint id);
    Horario? Get(uint id);
    IEnumerable<Horario> GetAll();
    IEnumerable<Horario> GetByProfissional(uint idProfissional);
    IEnumerable<Horario> GetByDiaSemana(string diaSemana, uint idProfissional);
}
