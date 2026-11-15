using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.DTO;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers
{
    [TestClass]
    public class DashboardControllerTests
    {
        private Mock<IDashboardService> dashboardServiceMock = null!;
        private Mock<ICategoriaService> categoriaServiceMock = null!;
        private Mock<IServicoService> servicoServiceMock = null!;
        private DashboardController controller = null!;

        [TestInitialize]
        public void Inicializar()
        {
            dashboardServiceMock = new Mock<IDashboardService>();
            categoriaServiceMock = new Mock<ICategoriaService>();
            servicoServiceMock = new Mock<IServicoService>();

            dashboardServiceMock
                .Setup(s => s.GetResumoServicos(1))
                .Returns(new DashboardResumoServicosDTO
                {
                    ServicoMaisUsadoNome = "Corte",
                    ServicoMaisUsadoQuantidade = 22,
                    CategoriaMaisUsadaNome = "Manicure",
                    CategoriaMaisUsadaQuantidade = 52,
                    ServicoMenosUsadoNome = "Depilação",
                    ServicoMenosUsadoQuantidade = 2
                });

            dashboardServiceMock
                .Setup(s => s.GetResumoFaturamento(1, null))
                .Returns(new DashboardResumoFaturamentoDTO
                {
                    FaturamentoMesAtual = 3531m,
                    PercentualVariacaoMensal = 10m,
                    FaturamentoSemanal = 1231m,
                    PercentualVariacaoSemanal = 17m,
                    ServicoMaisFaturaNome = "Corte",
                    ServicoMaisFaturaValor = 990m,
                    ServicoMenosFaturaNome = "Depilação",
                    ServicoMenosFaturaValor = 100m
                });

            categoriaServiceMock
                .Setup(s => s.GetAll(1))
                .Returns(new List<Categorium>
                {
                    new Categorium { Id = 1, IdEstabelecimento = 1, Nome = "Massagista" }
                });

            servicoServiceMock
                .Setup(s => s.GetAll(1))
                .Returns(new List<Servico>
                {
                    new Servico { Id = 1, IdCategoria = 1, IdEstabelecimento = 1, Nome = "Massagem Relaxante" }
                });

            controller = new DashboardController(
                dashboardServiceMock.Object,
                categoriaServiceMock.Object,
                servicoServiceMock.Object);
        }

        [TestMethod]
        public void Index_QuandoChamado_DeveRedirecionarParaServicos()
        {
            // Act
            var resultado = controller.Index();

            // Assert
            var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
            Assert.AreEqual(nameof(DashboardController.Servicos), redirect.ActionName);
        }

        [TestMethod]
        public void Servicos_Get_DeveRetornarViewComResumoEListasCarregadas()
        {
            // Act
            var resultado = controller.Servicos();

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardServicosViewModel>(viewResult.ViewData.Model);

            Assert.AreEqual("Corte", model.Resumo.ServicoMaisUsadoNome);
            Assert.AreEqual(1, model.CategoriasDisponiveis.Count());
            Assert.AreEqual(1, model.ServicosDisponiveis.Count());
        }

        [TestMethod]
        public void Servicos_Post_PorCategoria_QuandoExistemDados_DeveRetornarViewComItensETotal()
        {
            // Arrange
            var inicio = new DateTime(2026, 5, 1);
            var fim = new DateTime(2026, 5, 31);

            dashboardServiceMock
                .Setup(s => s.GetDistribuicaoPorCategoria(1, 1, "Genero", inicio, fim))
                .Returns(new List<DashboardItemDTO>
                {
                    new DashboardItemDTO { Label = "Mulher", Quantidade = 26, Percentual = 65m },
                    new DashboardItemDTO { Label = "Homem", Quantidade = 14, Percentual = 35m }
                });

            var input = new DashboardServicosViewModel
            {
                FiltroDemografico = "Genero",
                TipoFiltroItem = "Categoria",
                IdItemSelecionado = 1,
                DataInicio = inicio,
                DataFim = fim
            };

            // Act
            var resultado = controller.Servicos(input);

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardServicosViewModel>(viewResult.ViewData.Model);

            Assert.IsTrue(model.PesquisaRealizada);
            Assert.IsNull(model.MensagemErro);
            Assert.AreEqual(2, model.ItensGrafico.Count);
            Assert.AreEqual(40, model.TotalAtendimentos);
        }

        [TestMethod]
        public void Servicos_Post_QuandoNaoHaRegistros_DeveRetornarMensagemDoFluxoDeExcecao()
        {
            // Arrange
            var inicio = new DateTime(2026, 5, 1);
            var fim = new DateTime(2026, 5, 31);

            dashboardServiceMock
                .Setup(s => s.GetDistribuicaoPorServico(1, 1, "Genero", inicio, fim))
                .Returns(Enumerable.Empty<DashboardItemDTO>());

            var input = new DashboardServicosViewModel
            {
                FiltroDemografico = "Genero",
                TipoFiltroItem = "Servico",
                IdItemSelecionado = 1,
                DataInicio = inicio,
                DataFim = fim
            };

            // Act
            var resultado = controller.Servicos(input);

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardServicosViewModel>(viewResult.ViewData.Model);

            Assert.IsTrue(model.PesquisaRealizada);
            Assert.AreEqual(DashboardController.MensagemSemDados, model.MensagemErro);
        }

        [TestMethod]
        public void Faturamento_Get_DeveRetornarViewComResumoDeFaturamento()
        {
            // Act
            var resultado = controller.Faturamento();

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardFaturamentoViewModel>(viewResult.ViewData.Model);

            Assert.AreEqual(3531m, model.Resumo.FaturamentoMesAtual);
            Assert.AreEqual(1231m, model.Resumo.FaturamentoSemanal);
        }

        [TestMethod]
        public void Faturamento_Post_PorProfissional_QuandoExistemDados_DeveRetornarViewComItens()
        {
            // Arrange
            var inicio = new DateTime(2026, 5, 1);
            var fim = new DateTime(2026, 5, 31);

            dashboardServiceMock
                .Setup(s => s.GetFaturamentoPorProfissional(1, inicio, fim))
                .Returns(new List<DashboardItemDTO>
                {
                    new DashboardItemDTO { Label = "Fulano de Tal", Valor = 1960m },
                    new DashboardItemDTO { Label = "Ciclano da Silva", Valor = 1284m }
                });

            var input = new DashboardFaturamentoViewModel
            {
                FiltrarPor = "Profissional",
                DataInicio = inicio,
                DataFim = fim
            };

            // Act
            var resultado = controller.Faturamento(input);

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardFaturamentoViewModel>(viewResult.ViewData.Model);

            Assert.IsTrue(model.PesquisaRealizada);
            Assert.IsNull(model.MensagemErro);
            Assert.AreEqual(2, model.ItensGrafico.Count);
            Assert.AreEqual(1960m, model.ItensGrafico[0].Valor);
        }

        [TestMethod]
        public void Faturamento_Post_QuandoNaoHaRegistros_DeveRetornarMensagemDoFluxoDeExcecao()
        {
            // Arrange
            var inicio = new DateTime(2026, 5, 1);
            var fim = new DateTime(2026, 5, 31);

            dashboardServiceMock
                .Setup(s => s.GetFaturamentoPorCategoria(1, inicio, fim))
                .Returns(Enumerable.Empty<DashboardItemDTO>());

            var input = new DashboardFaturamentoViewModel
            {
                FiltrarPor = "Categoria",
                DataInicio = inicio,
                DataFim = fim
            };

            // Act
            var resultado = controller.Faturamento(input);

            // Assert
            var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
            var model = Assert.IsInstanceOfType<DashboardFaturamentoViewModel>(viewResult.ViewData.Model);

            Assert.IsTrue(model.PesquisaRealizada);
            Assert.AreEqual(DashboardController.MensagemSemDados, model.MensagemErro);
        }
    }
}
