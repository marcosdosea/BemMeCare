namespace Core.Service
{
    public interface IServicoService
    {
        uint Create(Servico servico);
        void Edit(Servico servico);
        void Delete(uint id);
        Servico Get(uint id);
        IEnumerable<Servico> GetAll();
        IEnumerable<Servico> GetByNome(string nome);
    }
}
