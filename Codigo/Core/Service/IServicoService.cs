namespace Core.Service
{
    public interface IServicoService
    {
        uint Create(Servico servico, uint idEstabelecimento);
        void Edit(Servico servico, uint idEstabelecimento);
        void Delete(uint id, uint idEstabelecimento);
        Servico? Get(uint id, uint idEstabelecimento);
        IEnumerable<Servico> GetAll(uint idEstabelecimento);
        IEnumerable<Servico> GetByNome(string nome, uint idEstabelecimento);
    }
}
