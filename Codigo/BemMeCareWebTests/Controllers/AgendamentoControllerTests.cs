using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.DTO;
using Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class AgendamentoControllerTests
{
    private Mock<IProfissionalService> profissionalServiceMock = null!;
    private Mock<IAgendamentoService> agendamentoServiceMock = null!;
    private Mock<IHorarioService> horarioServiceMock = null!;
    private Mock<IPessoaService> pessoaServiceMock = null!;
    private AgendamentoController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        profissionalServiceMock = new Mock<IProfissionalService>();
        agendamentoServiceMock = new Mock<IAgendamentoService>();
        horarioServiceMock = new Mock<IHorarioService>();
        pessoaServiceMock = new Mock<IPessoaService>();

        var dataAlvo = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horarioAlvo = new TimeOnly(9, 0);

        profissionalServiceMock
            .Setup(service => service.GetServicosProfissionais(1))
            .Returns(ObterProfissionaisServicosDeTeste());

        pessoaServiceMock
            .Setup(service => service.GetNome(1))
            .Returns("Ana Paula");

        horarioServiceMock
            .Setup(service => service.GetByProfissionalNaData(1, dataAlvo, 1))
            .Returns([CriarHorarioDeTeste(1, dataAlvo)]);

        agendamentoServiceMock
            .Setup(service => service.ValidateProfissionalServicosAgendamento(
                It.Is<SelecaoAgendamentoDTO>(dto =>
                    dto.IdProfissional == 1 &&
                    dto.IdServicosSelecionados.SequenceEqual(new[] { 10u })),
                1))
            .Returns(true);

        agendamentoServiceMock
            .Setup(service => service.ValidateProfissionalServicosAgendamento(
                It.Is<SelecaoAgendamentoDTO>(dto =>
                    dto.IdServicosSelecionados.Contains(999u)),
                1))
            .Returns(false);

        agendamentoServiceMock
            .Setup(service => service.GetHorariosDisponiveisProfissionalAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                It.IsAny<IEnumerable<Horario>>(),
                1))
            .Returns([horarioAlvo, new TimeOnly(10, 0)]);

        agendamentoServiceMock
            .Setup(service => service.GetResumoAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                1))
            .Returns(ObterResumoDeTeste(horarioAlvo));

        controller = new AgendamentoController(
            profissionalServiceMock.Object,
            agendamentoServiceMock.Object,
            horarioServiceMock.Object,
            pessoaServiceMock.Object)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>())
        };
    }

    [TestMethod]
    public void Servico_QuandoNaoHaServicosDeProfissionais_DeveRetornarListaVazia()
    {
        // Arrange
        profissionalServiceMock
            .Setup(service => service.GetServicosProfissionais(1))
            .Returns([]);

        var selecao = new SelecaoAgendamentoViewModel();

        // Act
        var resultado = controller.Servico(selecao);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var viewModel = Assert.IsInstanceOfType<AgendamentoServicosViewModel>(
            viewResult.ViewData.Model);

        Assert.IsEmpty(viewModel.Profissionais);
        Assert.AreSame(selecao, viewModel.Selecao);

        profissionalServiceMock.Verify(
            service => service.GetServicosProfissionais(1),
            Times.Once);
    }

    [TestMethod]
    public void Horario_QuandoProfissionalNaoTemHorarioDisponivel_DeveRetornarViewComMensagem()
    {
        // Arrange
        var dataAlvo = DateOnly.FromDateTime(DateTime.Now).AddDays(2);
        var selecao = new SelecaoAgendamentoViewModel
        {
            IdProfissional = 1,
            IdServicosSelecionados = [10u],
            DataDoAgendamento = dataAlvo,
            HorarioAgendamento = new TimeOnly(9, 0)
        };

        horarioServiceMock
            .Setup(service => service.GetByProfissionalNaData(1, dataAlvo, 1))
            .Returns([]);

        // Act
        var resultado = controller.Horario(selecao);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var viewModel = Assert.IsInstanceOfType<AgendamentoHorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.IsNull(viewModel.Selecao.HorarioAgendamento);
        Assert.AreEqual("Ana Paula", viewModel.NomeProfissional);
        Assert.IsEmpty(viewModel.Horarios);
        Assert.AreEqual("O profissional não atende nesta data.", viewModel.Mensagem);

        horarioServiceMock.Verify(
            service => service.GetByProfissionalNaData(1, dataAlvo, 1),
            Times.Once);
        agendamentoServiceMock.Verify(
            service => service.GetHorariosDisponiveisProfissionalAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                It.IsAny<IEnumerable<Horario>>(),
                It.IsAny<uint>()),
            Times.Never);
    }

    [TestMethod]
    public void Resumo_QuandoServicoNaoPertenceAoProfissional_DeveRedirecionarParaServicoComErro()
    {
        // Arrange
        var dataAlvo = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var selecao = new SelecaoAgendamentoViewModel
        {
            IdProfissional = 1,
            IdServicosSelecionados = [999u],
            DataDoAgendamento = dataAlvo,
            HorarioAgendamento = new TimeOnly(9, 0)
        };

        // Act
        var resultado = controller.Resumo(selecao);

        // Assert
        VerificarRedirecionamento(resultado, nameof(AgendamentoController.Servico));
        Assert.AreEqual(
            "A seleção de serviços não é válida. Escolha novamente.",
            controller.TempData["Erro"]);

        agendamentoServiceMock.Verify(
            service => service.ValidateProfissionalServicosAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                1),
            Times.Once);
        agendamentoServiceMock.Verify(
            service => service.GetResumoAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                It.IsAny<uint>()),
            Times.Never);
    }

    [TestMethod]
    public void Resumo_QuandoHorarioNaoEstaDisponivel_DeveRedirecionarParaHorarioComErro()
    {
        // Arrange
        var dataAlvo = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horarioIndisponivel = new TimeOnly(15, 0);
        var selecao = new SelecaoAgendamentoViewModel
        {
            IdProfissional = 1,
            IdServicosSelecionados = [10u],
            DataDoAgendamento = dataAlvo,
            HorarioAgendamento = horarioIndisponivel
        };

        // Act
        var resultado = controller.Resumo(selecao);

        // Assert
        var redirectResult = Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(nameof(AgendamentoController.Horario), redirectResult.ActionName);
        Assert.IsNull(selecao.HorarioAgendamento);
        Assert.AreEqual(
            "O horário escolhido não está mais disponível. Escolha outro.",
            controller.TempData["Erro"]);

        agendamentoServiceMock.Verify(
            service => service.GetResumoAgendamento(
                It.IsAny<SelecaoAgendamentoDTO>(),
                It.IsAny<uint>()),
            Times.Never);
    }

    [TestMethod]
    public void Create_QuandoDadosValidos_DeveCriarAgendamentoERedirecionarParaServico()
    {
        // Arrange
        var dataAlvo = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horarioAlvo = new TimeOnly(9, 0);
        var selecao = new SelecaoAgendamentoViewModel
        {
            IdProfissional = 1,
            IdServicosSelecionados = [10u],
            DataDoAgendamento = dataAlvo,
            HorarioAgendamento = horarioAlvo
        };

        SelecaoAgendamentoDTO? dtoEnviado = null;
        agendamentoServiceMock
            .Setup(service => service.Create(It.IsAny<SelecaoAgendamentoDTO>(), 1, 1))
            .Callback<SelecaoAgendamentoDTO, uint, uint>((dto, _, _) => dtoEnviado = dto)
            .Returns(1u);

        // Act
        var resultado = controller.Create(selecao);

        // Assert
        VerificarRedirecionamento(resultado, nameof(AgendamentoController.Servico));
        Assert.IsNotNull(dtoEnviado);
        Assert.AreEqual(selecao.IdProfissional, dtoEnviado.IdProfissional);
        Assert.IsTrue(dtoEnviado.IdServicosSelecionados.SequenceEqual(selecao.IdServicosSelecionados));
        Assert.AreEqual(selecao.DataDoAgendamento, dtoEnviado.DataDoAgendamento);
        Assert.AreEqual(selecao.HorarioAgendamento, dtoEnviado.HorarioAgendamento);

        agendamentoServiceMock.Verify(
            service => service.Create(It.IsAny<SelecaoAgendamentoDTO>(), 1, 1),
            Times.Once);
    }

    private static void VerificarRedirecionamento(ActionResult resultado, string actionName)
    {
        var redirectResult = Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(actionName, redirectResult.ActionName);
    }

    private static Horario CriarHorarioDeTeste(uint idProfissional, DateOnly data)
    {
        return new Horario
        {
            Id = 1,
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = data.ToDateTime(TimeOnly.MinValue),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = idProfissional,
            IdEstabelecimento = 1
        };
    }

    private static IEnumerable<ProfissionalServicosDTO> ObterProfissionaisServicosDeTeste()
    {
        return
        [
            new ProfissionalServicosDTO
            {
                Id = 1,
                Nome = "Ana Paula",
                Servicos =
                [
                    new ServicoProfissionalDTO
                    {
                        Id = 10,
                        Nome = "Corte Feminino",
                        Preco = 80.00m,
                        DuracaoEmMinutos = 60
                    }
                ]
            }
        ];
    }

    private static AgendamentoResumoDTO ObterResumoDeTeste(TimeOnly horario)
    {
        return new AgendamentoResumoDTO
        {
            NomeProfissional = "Ana Paula",
            Servicos =
            [
                new ServicoProfissionalDTO
                {
                    Id = 10,
                    Nome = "Corte Feminino",
                    Preco = 80.00m,
                    DuracaoEmMinutos = 60
                }
            ],
            PrecoTotal = 80.00m,
            DuracaoTotalEmMinutos = 60,
            HorarioAtendimento = horario
        };
    }
}