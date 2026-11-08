using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class ProfissionalControllerTests
{
    private Mock<IProfissionalService> profissionalServiceMock = null!;
    private ProfissionalController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        profissionalServiceMock = new Mock<IProfissionalService>();

        profissionalServiceMock
            .Setup(service => service.GetAll())
            .Returns(ObterProfissionaisDeTeste());

        profissionalServiceMock
            .Setup(service => service.Get(1))
            .Returns(ObterProfissionalAlvo());

        profissionalServiceMock
            .Setup(service => service.Get(999))
            .Returns((Profissional?)null);

        controller = new ProfissionalController(profissionalServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComTodosOsProfissionais()
    {
        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissionais = Assert.IsInstanceOfType<IEnumerable<ProfissionalViewModel>>(
            viewResult.ViewData.Model);

        Assert.AreEqual(3, profissionais.Count());

        var primeiroProfissional = profissionais.First();

        Assert.AreEqual(1u, primeiroProfissional.IdPessoa);
        Assert.AreEqual(1u, primeiroProfissional.IdEstabelecimento);
        Assert.AreEqual((sbyte)1, primeiroProfissional.GerirHorario);
        Assert.AreEqual((sbyte)1, primeiroProfissional.EstaAtivo);
        Assert.AreEqual((sbyte)1, primeiroProfissional.GerirServico);
        Assert.AreEqual((sbyte)0, primeiroProfissional.ReceberPagamentoDireto);
        Assert.AreEqual("prof1@pix.com", primeiroProfissional.ChavePix);
        Assert.AreEqual(15u, primeiroProfissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, primeiroProfissional.EProfissional);
        Assert.AreEqual((sbyte?)0, primeiroProfissional.EAtendente);

        profissionalServiceMock.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemProfissionais_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        profissionalServiceMock
            .Setup(service => service.GetAll())
            .Returns(Array.Empty<Profissional>());

        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissionais = Assert.IsInstanceOfType<IEnumerable<ProfissionalViewModel>>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0, profissionais.Count());

        profissionalServiceMock.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoProfissionalExiste_DeveRetornarViewComProfissional()
    {
        // Act
        var resultado = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, profissional.IdPessoa);
        Assert.AreEqual(1u, profissional.IdEstabelecimento);
        Assert.AreEqual((sbyte)1, profissional.GerirHorario);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
        Assert.AreEqual((sbyte)1, profissional.GerirServico);
        Assert.AreEqual((sbyte)0, profissional.ReceberPagamentoDireto);
        Assert.AreEqual("prof1@pix.com", profissional.ChavePix);
        Assert.AreEqual(15u, profissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, profissional.EProfissional);
        Assert.AreEqual((sbyte?)0, profissional.EAtendente);

        profissionalServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoProfissionalNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, profissional.IdPessoa);
        Assert.IsTrue(string.IsNullOrEmpty(profissional.ChavePix));

        profissionalServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornarViewComModeloInicial()
    {
        // Act
        var resultado = controller.Create();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, profissional.IdPessoa);
        Assert.AreEqual(0u, profissional.IdEstabelecimento);
        Assert.AreEqual((sbyte)0, profissional.GerirHorario);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
        Assert.AreEqual((sbyte)0, profissional.GerirServico);
        Assert.AreEqual((sbyte)0, profissional.ReceberPagamentoDireto);
        Assert.IsNull(profissional.ChavePix);
        Assert.AreEqual(0u, profissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, profissional.EProfissional);
        Assert.AreEqual((sbyte?)0, profissional.EAtendente);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarProfissionalERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovoProfissional();
        Profissional? profissionalCriado = null;

        profissionalServiceMock
            .Setup(service => service.Create(It.IsAny<Profissional>()))
            .Callback<Profissional>(p => profissionalCriado = p);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(profissionalCriado);
        Assert.AreEqual(modelo.IdPessoa, profissionalCriado.IdPessoa);
        Assert.AreEqual(modelo.IdEstabelecimento, profissionalCriado.IdEstabelecimento);
        Assert.AreEqual(modelo.GerirHorario, profissionalCriado.GerirHorario);
        Assert.AreEqual(modelo.EstaAtivo, profissionalCriado.EstaAtivo);
        Assert.AreEqual(modelo.GerirServico, profissionalCriado.GerirServico);
        Assert.AreEqual(modelo.ReceberPagamentoDireto, profissionalCriado.ReceberPagamentoDireto);
        Assert.AreEqual(modelo.ChavePix, profissionalCriado.ChavePix);
        Assert.AreEqual(modelo.DuracaoIntervaloTempo, profissionalCriado.DuracaoIntervaloTempo);
        Assert.AreEqual(modelo.EProfissional, profissionalCriado.EProfissional);
        Assert.AreEqual(modelo.EAtendente, profissionalCriado.EAtendente);

        profissionalServiceMock.Verify(
            service => service.Create(It.IsAny<Profissional>()),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarProfissional()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(ProfissionalViewModel.ChavePix),
            "Chave PIX inválida.");

        // Act
        var resultado = controller.Create(CriarModeloDeNovoProfissional());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        profissionalServiceMock.Verify(
            service => service.Create(It.IsAny<Profissional>()),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEProfissionalExiste_DeveRetornarViewComProfissional()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, profissional.IdPessoa);
        Assert.AreEqual(1u, profissional.IdEstabelecimento);
        Assert.AreEqual((sbyte)1, profissional.GerirHorario);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
        Assert.AreEqual((sbyte)1, profissional.GerirServico);
        Assert.AreEqual((sbyte)0, profissional.ReceberPagamentoDireto);
        Assert.AreEqual("prof1@pix.com", profissional.ChavePix);
        Assert.AreEqual(15u, profissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, profissional.EProfissional);
        Assert.AreEqual((sbyte?)0, profissional.EAtendente);

        profissionalServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEProfissionalNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, profissional.IdPessoa);
        Assert.IsTrue(string.IsNullOrEmpty(profissional.ChavePix));

        profissionalServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarProfissionalERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeProfissionalExistente();
        Profissional? profissionalEditado = null;

        profissionalServiceMock
            .Setup(service => service.Edit(It.IsAny<Profissional>()))
            .Callback<Profissional>(p => profissionalEditado = p);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(profissionalEditado);
        Assert.AreEqual(modelo.IdPessoa, profissionalEditado.IdPessoa);
        Assert.AreEqual(modelo.IdEstabelecimento, profissionalEditado.IdEstabelecimento);
        Assert.AreEqual(modelo.GerirHorario, profissionalEditado.GerirHorario);
        Assert.AreEqual(modelo.EstaAtivo, profissionalEditado.EstaAtivo);
        Assert.AreEqual(modelo.GerirServico, profissionalEditado.GerirServico);
        Assert.AreEqual(modelo.ReceberPagamentoDireto, profissionalEditado.ReceberPagamentoDireto);
        Assert.AreEqual(modelo.ChavePix, profissionalEditado.ChavePix);
        Assert.AreEqual(modelo.DuracaoIntervaloTempo, profissionalEditado.DuracaoIntervaloTempo);
        Assert.AreEqual(modelo.EProfissional, profissionalEditado.EProfissional);
        Assert.AreEqual(modelo.EAtendente, profissionalEditado.EAtendente);

        profissionalServiceMock.Verify(
            service => service.Edit(It.IsAny<Profissional>()),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarProfissional()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(ProfissionalViewModel.ChavePix),
            "Chave PIX inválida.");

        // Act
        var resultado = controller.Edit(CriarModeloDeProfissionalExistente());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        profissionalServiceMock.Verify(
            service => service.Edit(It.IsAny<Profissional>()),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEProfissionalExiste_DeveRetornarViewComProfissional()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, profissional.IdPessoa);
        Assert.AreEqual(1u, profissional.IdEstabelecimento);
        Assert.AreEqual((sbyte)1, profissional.GerirHorario);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
        Assert.AreEqual((sbyte)1, profissional.GerirServico);
        Assert.AreEqual((sbyte)0, profissional.ReceberPagamentoDireto);
        Assert.AreEqual("prof1@pix.com", profissional.ChavePix);
        Assert.AreEqual(15u, profissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, profissional.EProfissional);
        Assert.AreEqual((sbyte?)0, profissional.EAtendente);

        profissionalServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEProfissionalNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var profissional = Assert.IsInstanceOfType<ProfissionalViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, profissional.IdPessoa);
        Assert.IsTrue(string.IsNullOrEmpty(profissional.ChavePix));

        profissionalServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirProfissionalERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeProfissionalExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        profissionalServiceMock.Verify(
            service => service.Delete(modelo.IdPessoa),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(ProfissionalController.Index),
            redirectResult.ActionName);
    }

    private static ProfissionalViewModel CriarModeloDeNovoProfissional()
    {
        return new ProfissionalViewModel
        {
            IdPessoa = 4,
            IdEstabelecimento = 1,
            GerirHorario = 1,
            EstaAtivo = 1,
            GerirServico = 1,
            ReceberPagamentoDireto = 0,
            ChavePix = "prof4@pix.com",
            DuracaoIntervaloTempo = 15,
            EProfissional = 1,
            EAtendente = 0
        };
    }

    private static ProfissionalViewModel CriarModeloDeProfissionalExistente()
    {
        return new ProfissionalViewModel
        {
            IdPessoa = 2,
            IdEstabelecimento = 1,
            GerirHorario = 0,
            EstaAtivo = 1,
            GerirServico = 0,
            ReceberPagamentoDireto = 1,
            ChavePix = "prof2@pix.com",
            DuracaoIntervaloTempo = 30,
            EProfissional = 1,
            EAtendente = 0
        };
    }

    private static Profissional ObterProfissionalAlvo()
    {
        return new Profissional
        {
            IdPessoa = 1,
            IdEstabelecimento = 1,
            GerirHorario = 1,
            EstaAtivo = 1,
            GerirServico = 1,
            ReceberPagamentoDireto = 0,
            ChavePix = "prof1@pix.com",
            DuracaoIntervaloTempo = 15,
            EProfissional = 1,
            EAtendente = 0
        };
    }

    private static IEnumerable<Profissional> ObterProfissionaisDeTeste()
    {
        return
        [
            new Profissional
            {
                IdPessoa = 1,
                IdEstabelecimento = 1,
                GerirHorario = 1,
                EstaAtivo = 1,
                GerirServico = 1,
                ReceberPagamentoDireto = 0,
                ChavePix = "prof1@pix.com",
                DuracaoIntervaloTempo = 15,
                EProfissional = 1,
                EAtendente = 0
            },
            new Profissional
            {
                IdPessoa = 2,
                IdEstabelecimento = 1,
                GerirHorario = 0,
                EstaAtivo = 1,
                GerirServico = 0,
                ReceberPagamentoDireto = 1,
                ChavePix = "prof2@pix.com",
                DuracaoIntervaloTempo = 30,
                EProfissional = 1,
                EAtendente = 0
            },
            new Profissional
            {
                IdPessoa = 3,
                IdEstabelecimento = 1,
                GerirHorario = 1,
                EstaAtivo = 0,
                GerirServico = 1,
                ReceberPagamentoDireto = 1,
                ChavePix = "prof3@pix.com",
                DuracaoIntervaloTempo = 20,
                EProfissional = 1,
                EAtendente = 1
            }
        ];
    }
}
