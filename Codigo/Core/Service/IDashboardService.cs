using Core.DTO;

namespace Core.Service
{
    public interface IDashboardService
    {
        DashboardResumoServicosDTO GetResumoServicos(uint idEstabelecimento);

        IEnumerable<DashboardItemDTO> GetDistribuicaoPorServico(
            uint idEstabelecimento,
            uint idServico,
            string filtroDemografico,
            DateTime dataInicio,
            DateTime dataFim);

        IEnumerable<DashboardItemDTO> GetDistribuicaoPorCategoria(
            uint idEstabelecimento,
            uint idCategoria,
            string filtroDemografico,
            DateTime dataInicio,
            DateTime dataFim);

        DashboardResumoFaturamentoDTO GetResumoFaturamento(uint idEstabelecimento, DateTime? dataReferencia = null);

        IEnumerable<DashboardItemDTO> GetFaturamentoPorProfissional(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim);

        IEnumerable<DashboardItemDTO> GetFaturamentoPorServico(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim);

        IEnumerable<DashboardItemDTO> GetFaturamentoPorCategoria(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim);
    }
}
