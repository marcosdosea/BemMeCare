using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class AgendamentoDeClientesControllerTests
{
    private Mock<IAgendamentoDeClientesService> agendamentoServiceMock = null!;
    private Mock<IPessoaService> pessoaServiceMock = null!;
    private Mock<IServicoService> servicoServiceMock = null!;
    private Mock<IProfissionalService> profissionalServiceMock = null!;
    private AgendamentoDeClientesController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        agendamentoServiceMock = new Mock<IAgendamentoDeClientesService>();
        pessoaServiceMock = new Mock<IPessoaService>();
        servicoServiceMock = new Mock<IServicoService>();
        profissionalServiceMock = new Mock<IProfissionalService>();
        pessoaServiceMock.Setup(service => service.GetAll()).Returns([]);
        servicoServiceMock.Setup(service => service.GetAll(1)).Returns([]);
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([]);
        controller = new AgendamentoDeClientesController(
            agendamentoServiceMock.Object,
            pessoaServiceMock.Object,
            servicoServiceMock.Object,
            profissionalServiceMock.Object);
        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    [TestMethod]
    public void Index_QuandoSemFiltros_DeveBuscarDataAtualNoEstabelecimentoProvisorio()
    {
        agendamentoServiceMock
            .Setup(service => service.GetByData(
                It.IsAny<DateTime>(),
                1,
                null))
            .Returns([]);

        var resultado = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.IsInstanceOfType<IEnumerable<AgendamentoDeClientesViewModel>>(
            viewResult.ViewData.Model);
        agendamentoServiceMock.Verify(
            service => service.GetByData(DateTime.Today, 1, null),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoAgendamentoNaoExiste_DeveRetornarNotFound()
    {
        agendamentoServiceMock
            .Setup(service => service.Get(999, 1))
            .Returns((Agendamento?)null);

        var resultado = controller.Details(999);

        Assert.IsInstanceOfType<NotFoundResult>(resultado);
    }

    [TestMethod]
    public void Details_QuandoAgendamentoExiste_DeveRetornarViewComViewModel()
    {
        var agendamento = new Agendamento
        {
            Id = 12,
            IdPessoaClienteNavigation = new Pessoa { Nome = "Cliente", Telefone = "11987654321" },
            Profissional = new Profissional
            {
                IdPessoaNavigation = new Pessoa { Nome = "Profissional" }
            },
            Agendamentoservicos = []
        };
        agendamentoServiceMock.Setup(service => service.Get(12, 1)).Returns(agendamento);

        var resultado = controller.Details(12);

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(viewResult.Model);
        Assert.AreEqual("Cliente", viewModel.NomeCliente);
    }

    [TestMethod]
    public void Create_Get_DeveCarregarProfissionaisEServicosDoEstabelecimento()
    {
        pessoaServiceMock.Setup(service => service.GetAll()).Returns([
            new Pessoa { Id = 5, Nome = "Profissional" }
        ]);
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([
            new Profissional { IdPessoa = 5, IdEstabelecimento = 1, EstaAtivo = 1 },
            new Profissional { IdPessoa = 6, IdEstabelecimento = 2, EstaAtivo = 1 },
            new Profissional { IdPessoa = 7, IdEstabelecimento = 1, EstaAtivo = 0 }
        ]);
        servicoServiceMock.Setup(service => service.GetAll(1)).Returns([
            new Servico { Id = 10, Nome = "Corte", IdEstabelecimento = 1 }
        ]);

        var resultado = controller.Create();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(viewResult.Model);
        Assert.AreEqual("Profissional", viewModel.ProfissionaisDisponiveis.Single().Text);
        Assert.AreEqual("Corte", viewModel.ServicosDisponiveis.Single().Text);
        profissionalServiceMock.Verify(service => service.GetAll(), Times.Once);
        servicoServiceMock.Verify(service => service.GetAll(1), Times.Once);
    }

    [TestMethod]
    public void Create_QuandoPessoaNaoExiste_DeveExibirErroENaoCriarAgendamento()
    {
        pessoaServiceMock
            .Setup(service => service.GetByNome("Cliente"))
            .Returns([]);

        var resultado = controller.Create(CriarViewModel());

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.AreEqual("Cliente", ((AgendamentoDeClientesViewModel)viewResult.Model!).NomeCliente);
        Assert.IsTrue(controller.ModelState.ContainsKey(nameof(AgendamentoDeClientesViewModel.NomeCliente)));
        agendamentoServiceMock.Verify(
            service => service.Create(It.IsAny<Agendamento>(), It.IsAny<IEnumerable<uint>>()),
            Times.Never);
    }

    [TestMethod]
    public void Create_QuandoPessoaExiste_DeveAssociarClienteEUsarValoresDefinidosNoServidor()
    {
        var pessoaCliente = new Pessoa
        {
            Id = 42,
            Nome = "Cliente",
            Telefone = "11987654321"
        };
        pessoaServiceMock
            .Setup(service => service.GetByNome("Cliente"))
            .Returns([pessoaCliente]);
        var viewModel = CriarViewModel();
        viewModel.PrecoFinal = 9999m;
        viewModel.DuracaoMinutos = 999;
        viewModel.StatusAgendamento = "CO";

        var resultado = controller.Create(viewModel);

        Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        agendamentoServiceMock.Verify(
            service => service.Create(
                It.Is<Agendamento>(agendamento =>
                    agendamento.IdPessoaCliente == 42 &&
                    agendamento.IdEstabelecimento == 1 &&
                    agendamento.IdPessoaMarcou == 1 &&
                    agendamento.PrecoFinal == 0 &&
                    agendamento.DuracaoMinutos == 0 &&
                    agendamento.StatusAgendamento == null),
                It.Is<IEnumerable<uint>>(ids => ids.SequenceEqual(new uint[] { 10 }))),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModelStateInvalido_DeveRetornarViewSemBuscarPessoa()
    {
        var viewModel = CriarViewModel();
        controller.ModelState.AddModelError(nameof(viewModel.NomeCliente), "Nome inválido");

        var resultado = controller.Create(viewModel);

        Assert.IsInstanceOfType<ViewResult>(resultado);
        pessoaServiceMock.Verify(service => service.GetAll(), Times.Once);
        profissionalServiceMock.Verify(service => service.GetAll(), Times.Once);
        servicoServiceMock.Verify(service => service.GetAll(1), Times.Once);
        pessoaServiceMock.Verify(
            service => service.GetByNome(It.IsAny<string>()),
            Times.Never);
        agendamentoServiceMock.Verify(
            service => service.Create(It.IsAny<Agendamento>(), It.IsAny<IEnumerable<uint>>()),
            Times.Never);
    }

    [TestMethod]
    public void Create_QuandoServiceLancaExcecao_DeveRetornarViewComErroEOpcoes()
    {
        pessoaServiceMock.Setup(service => service.GetByNome("Cliente")).Returns([
            new Pessoa
            {
                Id = 42,
                Nome = "Cliente",
                Telefone = "11987654321"
            }
        ]);
        agendamentoServiceMock
            .Setup(service => service.Create(
                It.IsAny<Agendamento>(),
                It.IsAny<IEnumerable<uint>>()))
            .Throws(new InvalidOperationException("Horário indisponível."));

        var resultado = controller.Create(CriarViewModel());

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
        Assert.AreEqual("Horário indisponível.", controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(viewResult.Model);
        Assert.IsNotNull(viewModel.ProfissionaisDisponiveis);
        Assert.IsNotNull(viewModel.ServicosDisponiveis);
    }

    [TestMethod]
    public void Cancel_QuandoChamado_DeveDelegarAoServicoEVoltarParaIndex()
    {
        var resultado = controller.Cancel(12);

        Assert.AreEqual(nameof(AgendamentoDeClientesController.Index),
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado).ActionName);
        agendamentoServiceMock.Verify(service => service.Cancel(12, 1), Times.Once);
    }

    [TestMethod]
    public void Cancel_QuandoServiceLancaExcecao_DeveRedirecionarComMensagemDeErro()
    {
        agendamentoServiceMock
            .Setup(service => service.Cancel(12, 1))
            .Throws(new InvalidOperationException("Agendamento finalizado."));

        var resultado = controller.Cancel(12);

        Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.AreEqual("Agendamento finalizado.", controller.TempData["ErrorMessage"]);
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoChamado_DeveDelegarAoServicoEVoltarParaIndex()
    {
        var resultado = controller.ConfirmarSinal(12);

        Assert.AreEqual(nameof(AgendamentoDeClientesController.Index),
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado).ActionName);
        agendamentoServiceMock.Verify(
            service => service.ConfirmarSinal(12, 1),
            Times.Once);
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoServiceLancaExcecao_DeveRedirecionarComMensagemDeErro()
    {
        agendamentoServiceMock
            .Setup(service => service.ConfirmarSinal(12, 1))
            .Throws(new InvalidOperationException("Sinal não pode ser confirmado."));

        var resultado = controller.ConfirmarSinal(12);

        Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.AreEqual("Sinal não pode ser confirmado.", controller.TempData["ErrorMessage"]);
    }

    private static AgendamentoDeClientesViewModel CriarViewModel()
    {
        return new AgendamentoDeClientesViewModel
        {
            DataHoraAtendimento = new DateTime(2026, 12, 1, 10, 0, 0),
            IdsServicos = [10],
            IdProfissional = 1,
            NomeCliente = "Cliente",
            TelefoneCliente = "(11) 98765-4321"
        };
    }
}
