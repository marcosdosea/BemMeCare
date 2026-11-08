namespace Core.Service;

public interface IEstabelecimentoService
{
    uint Create(Estabelecimento estabelecimento);
    void Edit(Estabelecimento estabelecimento);
    void Delete(uint id);
    Estabelecimento Get(uint id);
    IEnumerable<Estabelecimento> GetAll();
    IEnumerable<Estabelecimento> GetByNome(string nome);
    IEnumerable<Estabelecimento> GetByPessoa(uint idPessoa);

}