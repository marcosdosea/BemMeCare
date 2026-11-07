using Core;

namespace Core.Service
{
    public interface IProfissionalService
    {
        uint Create(Profissional profissional);
        void Edit(Profissional profissional);
        void Delete(uint idProfissional);
        Profissional? Get(uint idProfissional);
        IEnumerable<Profissional> GetAll();
    }
}
