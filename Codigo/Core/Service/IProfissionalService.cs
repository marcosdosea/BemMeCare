using Core;
using Core.DTO;

namespace Core.Service
{
    public interface IProfissionalService
    {
        uint Create(Profissional profissional);
        void Edit(Profissional profissional);
        void Delete(uint idProfissional);
        Profissional? Get(uint idProfissional);
        IEnumerable<Profissional> GetAll();
        IEnumerable<Servicoprofissional> GetServicoProfissional(uint idProfissional, uint idEstabelecimento);
        Servicoprofissional? GetServicoProfissional(uint idProfissional, uint idServico, uint idEstabelecimento);
        void EditServicoProfissional(uint idProfissional, IEnumerable<Servicoprofissional> servicos);
        void CreateServicoProfissional(Servicoprofissional servicoProfissional);
        void UpdateDadosServicoProfissional(Servicoprofissional servicoProfissional);
        void DeleteServicoProfissional(uint idProfissional, uint idServico, uint idEstabelecimento);
        IEnumerable<ProfissionalServicosDTO> GetServicosProfissionais(uint idEstabelecimento);
    }
}