namespace Core.Service;

public interface ICategoriaService
{
    uint Create(Categorium categoria);
    void Edit(Categorium categoria);
    void Delete(uint id, uint idEstabelecimento);
    Categorium? Get(uint id, uint idEstabelecimento);
    IEnumerable<Categorium> GetAll(uint idEstabelecimento);
}