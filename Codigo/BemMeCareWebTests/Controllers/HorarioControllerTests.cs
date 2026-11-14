using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class HorarioControllerTests
{
    private const uint idEstabelecimentoEsperado = 1;

    private Mock<IHorarioService> horarioServiceMock = null!;
    private HorarioController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        horarioServiceMock = new Mock<IHorarioService>();

        horarioServiceMock
            .Setup(service => service.GetAll(idEstabelecimentoEsperado))
            .Returns(ObterHorariosDeTeste());

        horarioServiceMock
            .Setup(service => service.Get(1, idEstabelecimentoEsperado))
            .Returns(ObterHorarioAlvo());

        horarioServiceMock
            .Setup(service => service.Get(999, idEstabelecimentoEsperado))
            .Returns((Horario?)null);

        horarioServiceMock
            .Setup(service => service.GetByProfissional(1, idEstabelecimentoEsperado))
            .Returns(ObterHorariosDoProfissional());

        horarioServiceMock
            .Setup(service => service.GetByDiaSemana("Segunda-feira", 1, idEstabelecimentoEsperado))
            .Returns(new List<Horario> { ObterHorarioAlvo() });

        controller = new HorarioController(horarioServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoSemFiltros_DeveRetornarViewComTodosOsHorariosDoEstabelecimento()
    {
        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horarios = Assert.IsInstanceOfType<IEnumerable<HorarioViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(3, horarios);

        horarioServiceMock.Verify(
            service => service.GetAll(idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoFiltradoPorProfissionalEDia_DeveRetornarHorariosFiltrados()
    {
        // Act
        var resultado = controller.Index(1, "Segunda-feira");

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horarios = Assert.IsInstanceOfType<IEnumerable<HorarioViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(1, horarios);
        Assert.AreEqual(1u, horarios.First().Id);

        horarioServiceMock.Verify(
            service => service.GetByDiaSemana("Segunda-feira", 1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoFiltradoApenasPorProfissional_DeveRetornarHorariosDoProfissional()
    {
        // Act
        var resultado = controller.Index(1, null);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horarios = Assert.IsInstanceOfType<IEnumerable<HorarioViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(2, horarios);

        horarioServiceMock.Verify(
            service => service.GetByProfissional(1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoHorarioExiste_DeveRetornarViewComHorario()
    {
        // Act
        var resultado = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, horario.Id);
        Assert.AreEqual("Segunda-feira", horario.DiaSemana);
        Assert.AreEqual(idEstabelecimentoEsperado, horario.IdEstabelecimento);

        horarioServiceMock.Verify(
            service => service.Get(1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoHorarioNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, horario.Id);

        horarioServiceMock.Verify(
            service => service.Get(999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornarViewComModeloInicial()
    {
        // Act
        var resultado = controller.Create();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, horario.Id);
        Assert.AreEqual(DateTime.Today, horario.InicioVigencia);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarHorarioERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovoHorario();
        Horario? horarioCriado = null;

        horarioServiceMock
            .Setup(service => service.Create(It.IsAny<Horario>()))
            .Callback<Horario>(h => horarioCriado = h)
            .Returns(4u);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(horarioCriado);
        Assert.AreEqual(modelo.DiaSemana, horarioCriado.DiaSemana);
        Assert.AreEqual(modelo.InicioTurno, horarioCriado.InicioTurno);
        Assert.AreEqual(modelo.FimTurno, horarioCriado.FimTurno);
        Assert.AreEqual(idEstabelecimentoEsperado, horarioCriado.IdEstabelecimento);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarERetornarViewComModelo()
    {
        // Arrange
        var modelo = CriarModeloDeNovoHorario();
        controller.ModelState.AddModelError("DiaSemana", "Campo obrigatório");

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.AreEqual(modelo, viewResult.ViewData.Model);

        horarioServiceMock.Verify(
            service => service.Create(It.IsAny<Horario>()),
            Times.Never);
    }

    [TestMethod]
    public void Create_QuandoOcorreChoqueDeHorarios_DeveAdicionarErroNoModelStateERetornarView()
    {
        // Arrange
        var modelo = CriarModeloDeNovoHorario();
        horarioServiceMock
            .Setup(service => service.Create(It.IsAny<Horario>()))
            .Throws(new InvalidOperationException("Já existe um horário conflitante."));

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.AreEqual(modelo, viewResult.ViewData.Model);
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEHorarioExiste_DeveRetornarViewComHorario()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, horario.Id);
        Assert.AreEqual("Segunda-feira", horario.DiaSemana);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEHorarioNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, horario.Id);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarHorarioERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeHorarioExistente();
        Horario? horarioAtualizado = null;

        horarioServiceMock
            .Setup(service => service.Edit(It.IsAny<Horario>(), idEstabelecimentoEsperado))
            .Callback<Horario, uint>((h, idEst) => horarioAtualizado = h);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(horarioAtualizado);
        Assert.AreEqual(modelo.Id, horarioAtualizado.Id);
        Assert.AreEqual(idEstabelecimentoEsperado, horarioAtualizado.IdEstabelecimento);

        horarioServiceMock.Verify(
            service => service.Edit(It.IsAny<Horario>(), idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveEditarERetornarViewComModelo()
    {
        // Arrange
        var modelo = CriarModeloDeHorarioExistente();
        controller.ModelState.AddModelError("DiaSemana", "Campo obrigatório");

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.AreEqual(modelo, viewResult.ViewData.Model);

        horarioServiceMock.Verify(
            service => service.Edit(It.IsAny<Horario>(), It.IsAny<uint>()),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoOcorreChoqueDeHorarios_DeveAdicionarErroNoModelStateERetornarView()
    {
        // Arrange
        var modelo = CriarModeloDeHorarioExistente();
        horarioServiceMock
            .Setup(service => service.Edit(It.IsAny<Horario>(), idEstabelecimentoEsperado))
            .Throws(new InvalidOperationException("Choque de horário."));

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        Assert.AreEqual(modelo, viewResult.ViewData.Model);
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEHorarioExiste_DeveRetornarViewComHorario()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, horario.Id);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEHorarioNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var horario = Assert.IsInstanceOfType<HorarioViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, horario.Id);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirHorarioERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeHorarioExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        horarioServiceMock.Verify(
            service => service.Delete(modelo.Id, idEstabelecimentoEsperado),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult = Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(nameof(HorarioController.Index), redirectResult.ActionName);
    }

    private static HorarioViewModel CriarModeloDeNovoHorario()
    {
        return new HorarioViewModel
        {
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Quinta-feira",
            IdProfissional = 1
        };
    }

    private static HorarioViewModel CriarModeloDeHorarioExistente()
    {
        return new HorarioViewModel
        {
            Id = 1,
            InicioTurno = new TimeSpan(9, 0, 0),
            FimTurno = new TimeSpan(17, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = idEstabelecimentoEsperado
        };
    }

    private static Horario ObterHorarioAlvo()
    {
        return new Horario
        {
            Id = 1,
            InicioTurno = new TimeSpan(9, 0, 0),
            FimTurno = new TimeSpan(17, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = idEstabelecimentoEsperado
        };
    }

    private static IEnumerable<Horario> ObterHorariosDoProfissional()
    {
        return
        [
            ObterHorarioAlvo(),
            new Horario
            {
                Id = 2,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Terça-feira",
                IdProfissional = 1,
                IdEstabelecimento = idEstabelecimentoEsperado
            }
        ];
    }

    private static IEnumerable<Horario> ObterHorariosDeTeste()
    {
        return
        [
            ObterHorarioAlvo(),
            new Horario
            {
                Id = 2,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Terça-feira",
                IdProfissional = 1,
                IdEstabelecimento = idEstabelecimentoEsperado
            },
            new Horario
            {
                Id = 3,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Quarta-feira",
                IdProfissional = 2,
                IdEstabelecimento = idEstabelecimentoEsperado
            }
        ];
    }
}
