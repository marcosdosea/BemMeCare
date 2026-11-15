using BemMeCareWeb.Models;
using Core;
using Core.DTO;
using Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class DashboardController : Controller
    {
        public const string MensagemSemDados = "Não foram encontrados dados correspondentes aos filtros aplicados";

        private readonly IDashboardService dashboardService;
        private readonly ICategoriaService categoriaService;
        private readonly IServicoService servicoService;
        private readonly uint idEstabelecimentoProvisorio = 1;

        public DashboardController(
            IDashboardService dashboardService,
            ICategoriaService categoriaService,
            IServicoService servicoService)
        {
            this.dashboardService = dashboardService;
            this.categoriaService = categoriaService;
            this.servicoService = servicoService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction(nameof(Servicos));
        }

        [HttpGet]
        public ActionResult Servicos()
        {
            var viewModel = new DashboardServicosViewModel
            {
                Resumo = dashboardService.GetResumoServicos(idEstabelecimentoProvisorio)
            };

            CarregarListasServicos(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Servicos(DashboardServicosViewModel viewModel)
        {
            viewModel.Resumo = dashboardService.GetResumoServicos(idEstabelecimentoProvisorio);
            CarregarListasServicos(viewModel);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                IEnumerable<DashboardItemDTO> resultado;
                string nomeItem;
                string nomeTipo;

                if (string.Equals(viewModel.TipoFiltroItem, "Categoria", StringComparison.OrdinalIgnoreCase))
                {
                    resultado = dashboardService.GetDistribuicaoPorCategoria(
                        idEstabelecimentoProvisorio,
                        viewModel.IdItemSelecionado,
                        viewModel.FiltroDemografico,
                        viewModel.DataInicio,
                        viewModel.DataFim);

                    nomeTipo = "a categoria";
                    nomeItem = viewModel.CategoriasDisponiveis
                        .FirstOrDefault(c => c.Id == viewModel.IdItemSelecionado)?.Nome ?? string.Empty;
                }
                else
                {
                    resultado = dashboardService.GetDistribuicaoPorServico(
                        idEstabelecimentoProvisorio,
                        viewModel.IdItemSelecionado,
                        viewModel.FiltroDemografico,
                        viewModel.DataInicio,
                        viewModel.DataFim);

                    nomeTipo = "o serviço";
                    nomeItem = viewModel.ServicosDisponiveis
                        .FirstOrDefault(s => s.Id == viewModel.IdItemSelecionado)?.Nome ?? string.Empty;
                }

                viewModel.ItensGrafico = resultado.ToList();
                viewModel.TotalAtendimentos = viewModel.ItensGrafico.Sum(x => x.Quantidade);
                viewModel.PesquisaRealizada = true;

                string labelDemografico = ObterLabelDemografico(viewModel.FiltroDemografico);
                viewModel.TituloGrafico = $"Percentual de {labelDemografico} que usa {nomeTipo} {nomeItem}".Trim();

                if (!viewModel.ItensGrafico.Any())
                {
                    viewModel.MensagemErro = MensagemSemDados;
                }
            }
            catch (ArgumentException ex)
            {
                viewModel.MensagemErro = ex.Message;
            }

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Faturamento()
        {
            var viewModel = new DashboardFaturamentoViewModel
            {
                Resumo = dashboardService.GetResumoFaturamento(idEstabelecimentoProvisorio)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Faturamento(DashboardFaturamentoViewModel viewModel)
        {
            viewModel.Resumo = dashboardService.GetResumoFaturamento(idEstabelecimentoProvisorio);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                string chave = viewModel.FiltrarPor.Trim().ToUpperInvariant();
                IEnumerable<DashboardItemDTO> resultado;
                string sufixoTitulo;

                switch (chave)
                {
                    case "PROFISSIONAL":
                        resultado = dashboardService.GetFaturamentoPorProfissional(
                            idEstabelecimentoProvisorio, viewModel.DataInicio, viewModel.DataFim);
                        sufixoTitulo = "profissional";
                        break;
                    case "SERVICO" or "SERVIÇO":
                        resultado = dashboardService.GetFaturamentoPorServico(
                            idEstabelecimentoProvisorio, viewModel.DataInicio, viewModel.DataFim);
                        sufixoTitulo = "serviço";
                        break;
                    case "CATEGORIA":
                        resultado = dashboardService.GetFaturamentoPorCategoria(
                            idEstabelecimentoProvisorio, viewModel.DataInicio, viewModel.DataFim);
                        sufixoTitulo = "categoria";
                        break;
                    default:
                        throw new ArgumentException("Filtro de faturamento inválido.");
                }

                viewModel.ItensGrafico = resultado.ToList();
                viewModel.TituloGrafico = $"Faturamento por {sufixoTitulo}";
                viewModel.PesquisaRealizada = true;

                if (!viewModel.ItensGrafico.Any())
                {
                    viewModel.MensagemErro = MensagemSemDados;
                }
            }
            catch (ArgumentException ex)
            {
                viewModel.MensagemErro = ex.Message;
            }

            return View(viewModel);
        }

        private void CarregarListasServicos(DashboardServicosViewModel viewModel)
        {
            viewModel.CategoriasDisponiveis = categoriaService.GetAll(idEstabelecimentoProvisorio)?.ToList()
                ?? new List<Categorium>();

            viewModel.ServicosDisponiveis = servicoService.GetAll(idEstabelecimentoProvisorio)?.ToList()
                ?? new List<Servico>();
        }

        private static string ObterLabelDemografico(string filtroDemografico)
        {
            return filtroDemografico.Trim().ToUpperInvariant() switch
            {
                "GENERO" or "GÊNERO" => "gênero",
                "SEXUALIDADE" => "sexualidade",
                "COR" => "cor",
                _ => filtroDemografico.ToLowerInvariant()
            };
        }
    }
}
