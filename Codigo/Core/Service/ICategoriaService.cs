namespace Core.Service;

public interface ICategoriaService
{
    uint Create(Categorium categoria);
    void Edit(Categorium categoria);
    void Delete(uint id);
    Categorium? Get(uint id);
    IEnumerable<Categorium> GetAll();
}