using Core;
using Core.DTO;
using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models
{
    public class DashboardServicosViewModel
    {
        public DashboardResumoServicosDTO Resumo { get; set; } = new DashboardResumoServicosDTO();

        [Display(Name = "Filtrar por")]
        [Required(ErrorMessage = "Selecione o filtro demográfico.")]
        public string FiltroDemografico { get; set; } = "Genero";

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "Selecione se deseja filtrar por Categoria ou Serviços.")]
        public string TipoFiltroItem { get; set; } = "Categoria";

        [Display(Name = "Item")]
        [Range(1, uint.MaxValue, ErrorMessage = "Selecione uma categoria ou serviço válido.")]
        public uint IdItemSelecionado { get; set; }

        [Display(Name = "Data início")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [Display(Name = "Data fim")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        public DateTime DataFim { get; set; } = DateTime.Today;

        public IEnumerable<Categorium> CategoriasDisponiveis { get; set; } = new List<Categorium>();
        public IEnumerable<Servico> ServicosDisponiveis { get; set; } = new List<Servico>();

        public List<DashboardItemDTO> ItensGrafico { get; set; } = new List<DashboardItemDTO>();
        public int TotalAtendimentos { get; set; }
        public string TituloGrafico { get; set; } = string.Empty;
        public bool PesquisaRealizada { get; set; }
        public string? MensagemErro { get; set; }
    }

    public class DashboardFaturamentoViewModel
    {
        public DashboardResumoFaturamentoDTO Resumo { get; set; } = new DashboardResumoFaturamentoDTO();

        [Display(Name = "Filtrar por")]
        [Required(ErrorMessage = "Selecione o filtro de faturamento.")]
        public string FiltrarPor { get; set; } = "Profissional";

        [Display(Name = "Data início")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [Display(Name = "Data fim")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        public DateTime DataFim { get; set; } = DateTime.Today;

        public List<DashboardItemDTO> ItensGrafico { get; set; } = new List<DashboardItemDTO>();
        public string TituloGrafico { get; set; } = string.Empty;
        public bool PesquisaRealizada { get; set; }
        public string? MensagemErro { get; set; }
    }
}
