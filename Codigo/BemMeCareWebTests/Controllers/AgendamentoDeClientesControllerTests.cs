using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class AgendamentoDeClientesControllerTests
{
    private Mock<IAgendamentoService> agendamentoServiceMock = null!;
    private Mock<IPessoaService> pessoaServiceMock = null!;
    private AgendamentoDeClientesController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        agendamentoServiceMock = new Mock<IAgendamentoService>();
        pessoaServiceMock = new Mock<IPessoaService>();
        controller = new AgendamentoDeClientesController(
            agendamentoServiceMock.Object,
            pessoaServiceMock.Object);
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
        pessoaServiceMock.Verify(
            service => service.GetByNome(It.IsAny<string>()),
            Times.Never);
        agendamentoServiceMock.Verify(
            service => service.Create(It.IsAny<Agendamento>(), It.IsAny<IEnumerable<uint>>()),
            Times.Never);
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
    public void ConfirmarSinal_QuandoChamado_DeveDelegarAoServicoEVoltarParaIndex()
    {
        var resultado = controller.ConfirmarSinal(12);

        Assert.AreEqual(nameof(AgendamentoDeClientesController.Index),
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado).ActionName);
        agendamentoServiceMock.Verify(
            service => service.ConfirmarSinal(12, 1),
            Times.Once);
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
