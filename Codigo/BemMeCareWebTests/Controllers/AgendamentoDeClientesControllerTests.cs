using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.DTO;
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
    private Mock<IProfissionalService> profissionalServiceMock = null!;
    private AgendamentoDeClientesController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        agendamentoServiceMock = new Mock<IAgendamentoDeClientesService>();
        pessoaServiceMock = new Mock<IPessoaService>();
        profissionalServiceMock = new Mock<IProfissionalService>();
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([]);
        controller = new AgendamentoDeClientesController(
            agendamentoServiceMock.Object,
            pessoaServiceMock.Object,
            profissionalServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoSemFiltros_DeveExibirMesAtual()
    {
        var primeiroDiaDoMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var diasAntes = ((int)primeiroDiaDoMes.DayOfWeek + 6) % 7;
        var inicio = primeiroDiaDoMes.AddDays(-diasAntes);
        var diasDoMes = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        var quantidadeDeDias = ((diasAntes + diasDoMes + 6) / 7) * 7;
        var fimExclusivo = inicio.AddDays(quantidadeDeDias);
        agendamentoServiceMock
            .Setup(service => service.GetByPeriodo(
                inicio,
                fimExclusivo,
                1,
                null))
            .Returns([]);

        var resultado = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesAgendaViewModel>(viewResult.Model);
        Assert.AreEqual("mes", viewModel.Visualizacao);
        Assert.AreEqual(quantidadeDeDias / 7, viewModel.Semanas.Count);
        agendamentoServiceMock.Verify(
            service => service.GetByPeriodo(inicio, fimExclusivo, 1, null),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoVisualizacaoSemana_DeveBuscarSemanaIniciadaNaSegunda()
    {
        var data = new DateTime(2026, 12, 2);
        var inicio = new DateTime(2026, 11, 30);
        var fimExclusivo = inicio.AddDays(7);
        agendamentoServiceMock
            .Setup(service => service.GetByPeriodo(inicio, fimExclusivo, 1, null))
            .Returns([]);

        var resultado = controller.Index(data, "semana");

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesAgendaViewModel>(
            Assert.IsInstanceOfType<ViewResult>(resultado).Model);
        Assert.AreEqual(data.AddDays(-7), viewModel.DataAnterior);
        Assert.AreEqual(data.AddDays(7), viewModel.DataProxima);
        Assert.AreEqual(7, viewModel.Semanas.Single().Count);
        agendamentoServiceMock.Verify(
            service => service.GetByPeriodo(inicio, fimExclusivo, 1, null),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoVisualizacaoDia_DeveBuscarSomenteODiaSelecionado()
    {
        var data = new DateTime(2026, 12, 2);
        agendamentoServiceMock
            .Setup(service => service.GetByPeriodo(data, data.AddDays(1), 1, null))
            .Returns([]);

        var resultado = controller.Index(data, "dia");

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesAgendaViewModel>(
            Assert.IsInstanceOfType<ViewResult>(resultado).Model);
        Assert.AreEqual(1, viewModel.Semanas.Single().Count);
        agendamentoServiceMock.Verify(
            service => service.GetByPeriodo(data, data.AddDays(1), 1, null),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNavegaMes_DeveManterAReferenciaNoInicioDosMeses()
    {
        var data = new DateTime(2026, 1, 31);
        agendamentoServiceMock
            .Setup(service => service.GetByPeriodo(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                1,
                null))
            .Returns([]);

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesAgendaViewModel>(
            Assert.IsInstanceOfType<ViewResult>(controller.Index(data)).Model);

        Assert.AreEqual(new DateTime(2025, 12, 1), viewModel.DataAnterior);
        Assert.AreEqual(new DateTime(2026, 2, 1), viewModel.DataProxima);
    }

    [TestMethod]
    public void Index_QuandoVisualizacaoInvalida_DeveRetornarBadRequest()
    {
        var resultado = controller.Index(visualizacao: "ano");

        Assert.IsInstanceOfType<BadRequestResult>(resultado);
        agendamentoServiceMock.Verify(
            service => service.GetByPeriodo(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<uint>(),
                It.IsAny<uint?>()),
            Times.Never);
    }

    [TestMethod]
    public void Create_Get_DeveListarCadaServicoComSeuProfissional()
    {
        pessoaServiceMock.Setup(service => service.GetAll()).Returns([
            new Pessoa { Id = 5, Nome = "Ana" },
            new Pessoa { Id = 6, Nome = "Bia" },
            new Pessoa { Id = 7, Nome = "Caio" },
            new Pessoa { Id = 8, Nome = "Inativo" },
            new Pessoa { Id = 9, Nome = "Outro estabelecimento" }
        ]);
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([
            CriarProfissionalComServico(5, 1, 1),
            CriarProfissionalComServico(6, 1, 1),
            CriarProfissionalComServico(7, 1, 1),
            CriarProfissionalComServico(8, 1, 0),
            CriarProfissionalComServico(9, 2, 1)
        ]);

        var resultado = controller.Create();

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(
            Assert.IsInstanceOfType<ViewResult>(resultado).Model);
        var opcoes = viewModel.OpcoesServicoProfissional.ToList();
        Assert.AreEqual(3, opcoes.Count);
        CollectionAssert.AreEquivalent(
            new[] { "Ana", "Bia", "Caio" },
            opcoes.Select(opcao => opcao.NomeProfissional).ToArray());
        Assert.IsTrue(opcoes.All(opcao => opcao.NomeServico == "Corte" && opcao.IdServico == 10));
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
    public void Horarios_QuandoServicoProfissionalEDataSaoValidos_DeveExibirHorariosDisponiveis()
    {
        var data = DateTime.Today.AddDays(1);
        var horarios = new[]
        {
            new HorarioDisponivelDTO { Horario = data.AddHours(10) },
            new HorarioDisponivelDTO { Horario = data.AddHours(10).AddMinutes(30) }
        };
        agendamentoServiceMock
            .Setup(service => service.GetHorariosDisponiveis(5, 10, data.Date, 1))
            .Returns(horarios);
        pessoaServiceMock.Setup(service => service.GetAll()).Returns([
            new Pessoa { Id = 5, Nome = "Ana" }
        ]);
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([
            CriarProfissionalComServico(5, 1, 1)
        ]);

        var resultado = controller.Horarios(5, 10, data);

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(
            Assert.IsInstanceOfType<ViewResult>(resultado).Model);
        Assert.AreEqual(2, viewModel.HorariosDisponiveis.Count());
        Assert.AreEqual("Ana", viewModel.NomeProfissional);
        Assert.AreEqual("Corte", viewModel.NomesServicos);
    }

    [TestMethod]
    public void Resumo_QuandoHorarioEstaDisponivel_DeveExibirResumoSemCadastrarCliente()
    {
        var dataHora = DateTime.Today.AddDays(1).AddHours(10);
        var resumo = new AgendamentoDTO
        {
            NomeProfissional = "Ana",
            Servicos = ["Corte"],
            HorarioAtendimento = dataHora,
            DuracaoTotalEmMinutos = 30,
            PrecoTotal = 80m
        };
        agendamentoServiceMock
            .Setup(service => service.CriarResumo(5, 10, dataHora, 1))
            .Returns(resumo);
        pessoaServiceMock.Setup(service => service.GetAll()).Returns([
            new Pessoa { Id = 5, Nome = "Ana" }
        ]);
        profissionalServiceMock.Setup(service => service.GetAll()).Returns([
            CriarProfissionalComServico(5, 1, 1)
        ]);

        var resultado = controller.Resumo(5, 10, dataHora);

        var viewModel = Assert.IsInstanceOfType<AgendamentoDeClientesViewModel>(
            Assert.IsInstanceOfType<ViewResult>(resultado).Model);
        Assert.AreEqual(30u, viewModel.DuracaoMinutos);
        Assert.AreEqual(80m, viewModel.PrecoFinal);
        Assert.IsTrue(string.IsNullOrEmpty(viewModel.NomeCliente));
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

    private static Profissional CriarProfissionalComServico(
        uint idPessoa,
        uint idEstabelecimento,
        sbyte estaAtivo)
    {
        return new Profissional
        {
            IdPessoa = idPessoa,
            IdEstabelecimento = idEstabelecimento,
            EstaAtivo = estaAtivo,
            Servicoprofissionals =
            [
                new Servicoprofissional
                {
                    IdProfissional = idPessoa,
                    IdEstabelecimento = idEstabelecimento,
                    IdServico = 10,
                    IdServicoNavigation = new Servico { Id = 10, Nome = "Corte" }
                }
            ]
        };
    }
}
