using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class ServicoControllerTests
{
    private Mock<IServicoService> servicoServiceMock = null!;
    private Mock<ICategoriaService> categoriaServiceMock = null!;
    private ServicoController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        servicoServiceMock = new Mock<IServicoService>();
        categoriaServiceMock = new Mock<ICategoriaService>();

        servicoServiceMock
            .Setup(service => service.GetAll(1))
            .Returns(ObterServicosDeTeste().Where(item => item.IdEstabelecimento == 1));

        servicoServiceMock
            .Setup(service => service.Get(1, 1))
            .Returns(ObterServicoAlvo());

        servicoServiceMock
            .Setup(service => service.Get(999, 1))
            .Returns((Servico?)null);

        servicoServiceMock
            .Setup(service => service.Get(3, 1))
            .Returns((Servico?)null);

        servicoServiceMock
            .Setup(service => service.GetByNome("Cor", 1))
            .Returns([ObterServicoAlvo()]);

        categoriaServiceMock
            .Setup(service => service.GetAll())
            .Returns(ObterCategoriasDeTeste());

        controller = new ServicoController(
            servicoServiceMock.Object,
            categoriaServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarSomenteServicosDoEstabelecimento()
    {
        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servicos = Assert.IsInstanceOfType<IEnumerable<ServicoViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(2, servicos);
        Assert.IsTrue(servicos.All(item => item.IdEstabelecimento == 1));

        var primeiroServico = servicos.First();

        Assert.AreEqual(1u, primeiroServico.Id);
        Assert.AreEqual("Corte", primeiroServico.Nome);
        Assert.AreEqual(1u, primeiroServico.IdCategoria);
        Assert.AreEqual(1u, primeiroServico.IdEstabelecimento);

        servicoServiceMock.Verify(
            service => service.GetAll(1),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoInformadoNome_DevePesquisarPeloNome()
    {
        var resultado = controller.Index(" Cor ");

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servicos = Assert.IsInstanceOfType<IEnumerable<ServicoViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(1, servicos);
        Assert.AreEqual("Corte", servicos.First().Nome);
        Assert.AreEqual("Cor", viewResult.ViewData["NomePesquisado"]);
        servicoServiceMock.Verify(
            service => service.GetByNome("Cor", 1),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemServicos_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        servicoServiceMock
            .Setup(service => service.GetAll(1))
            .Returns(Array.Empty<Servico>());

        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servicos = Assert.IsInstanceOfType<IEnumerable<ServicoViewModel>>(
            viewResult.ViewData.Model);

        Assert.IsEmpty(servicos);

        servicoServiceMock.Verify(
            service => service.GetAll(1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoServicoExiste_DeveRetornarViewComServico()
    {
        // Act
        var resultado = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarServicoAlvo(servico);

        servicoServiceMock.Verify(
            service => service.Get(1, 1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoServicoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarModeloVazio(servico);

        servicoServiceMock.Verify(
            service => service.Get(999, 1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoServicoPertenceAOutroEstabelecimento_DeveRetornarViewComModeloVazio()
    {
        var resultado = controller.Details(3);

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarModeloVazio(servico);
        servicoServiceMock.Verify(
            service => service.Get(3, 1),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornarViewComModeloInicial()
    {
        // Act
        var resultado = controller.Create();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarModeloVazio(servico);
        var categorias = Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(
            viewResult.ViewData["Categorias"]);
        Assert.HasCount(2, categorias);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarServicoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovoServico();
        Servico? servicoCriado = null;

        servicoServiceMock
            .Setup(service => service.Create(It.IsAny<Servico>(), 1))
            .Callback<Servico, uint>((servico, _) => servicoCriado = servico);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(servicoCriado);
        VerificarConversao(modelo, servicoCriado);

        servicoServiceMock.Verify(
            service => service.Create(It.IsAny<Servico>(), 1),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarServico()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(ServicoViewModel.Nome),
            "O nome do serviço é obrigatório.");

        // Act
        var resultado = controller.Create(CriarModeloDeNovoServico());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        servicoServiceMock.Verify(
            service => service.Create(It.IsAny<Servico>(), 1),
            Times.Never);
    }

    [TestMethod]
    public void Create_QuandoCategoriaPertenceAOutroEstabelecimento_NaoDeveCriarServico()
    {
        var modelo = CriarModeloDeNovoServico();
        modelo.IdCategoria = 3;

        var resultado = controller.Create(modelo);

        VerificarRedirecionamentoParaIndex(resultado);
        servicoServiceMock.Verify(
            service => service.Create(It.IsAny<Servico>(), 1),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEServicoExiste_DeveRetornarViewComServico()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarServicoAlvo(servico);

        servicoServiceMock.Verify(
            service => service.Get(1, 1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEServicoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarModeloVazio(servico);

        servicoServiceMock.Verify(
            service => service.Get(999, 1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarServicoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeServicoExistente();
        Servico? servicoEditado = null;

        servicoServiceMock
            .Setup(service => service.Edit(It.IsAny<Servico>(), 1))
            .Callback<Servico, uint>((servico, _) => servicoEditado = servico);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(servicoEditado);
        VerificarConversao(modelo, servicoEditado);

        servicoServiceMock.Verify(
            service => service.Edit(It.IsAny<Servico>(), 1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarServico()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(ServicoViewModel.Nome),
            "O nome do serviço é obrigatório.");

        // Act
        var resultado = controller.Edit(CriarModeloDeServicoExistente());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        servicoServiceMock.Verify(
            service => service.Edit(It.IsAny<Servico>(), 1),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEServicoExiste_DeveRetornarViewComServico()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarServicoAlvo(servico);

        servicoServiceMock.Verify(
            service => service.Get(1, 1),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEServicoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var servico = Assert.IsInstanceOfType<ServicoViewModel>(
            viewResult.ViewData.Model);

        VerificarModeloVazio(servico);

        servicoServiceMock.Verify(
            service => service.Get(999, 1),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirServicoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeServicoExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        servicoServiceMock.Verify(
            service => service.Delete(modelo.Id, 1),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(ServicoController.Index),
            redirectResult.ActionName);
    }

    private static void VerificarServicoAlvo(ServicoViewModel servico)
    {
        Assert.AreEqual(1u, servico.Id);
        Assert.AreEqual("Corte", servico.Nome);
        Assert.AreEqual(1u, servico.IdCategoria);
        Assert.AreEqual(1u, servico.IdEstabelecimento);
    }

    private static void VerificarModeloVazio(ServicoViewModel servico)
    {
        Assert.AreEqual(0u, servico.Id);
        Assert.IsTrue(string.IsNullOrEmpty(servico.Nome));
        Assert.AreEqual(0u, servico.IdCategoria);
        Assert.AreEqual(0u, servico.IdEstabelecimento);
    }

    private static void VerificarConversao(ServicoViewModel modelo, Servico servico)
    {
        Assert.AreEqual(modelo.Id, servico.Id);
        Assert.AreEqual(modelo.Nome, servico.Nome);
        Assert.AreEqual(modelo.IdCategoria, servico.IdCategoria);
        Assert.AreEqual(modelo.IdEstabelecimento, servico.IdEstabelecimento);
    }

    private static ServicoViewModel CriarModeloDeNovoServico()
    {
        return new ServicoViewModel
        {
            Id = 4,
            Nome = "Massagem",
            IdCategoria = 2,
            IdEstabelecimento = 1
        };
    }

    private static ServicoViewModel CriarModeloDeServicoExistente()
    {
        return new ServicoViewModel
        {
            Id = 1,
            Nome = "Corte",
            IdCategoria = 1,
            IdEstabelecimento = 1
        };
    }

    private static Servico ObterServicoAlvo()
    {
        return new Servico
        {
            Id = 1,
            Nome = "Corte",
            IdCategoria = 1,
            IdEstabelecimento = 1
        };
    }

    private static IEnumerable<Servico> ObterServicosDeTeste()
    {
        return
        [
            ObterServicoAlvo(),
            new Servico
            {
                Id = 2,
                Nome = "Manicure",
                IdCategoria = 2,
                IdEstabelecimento = 1
            },
            new Servico
            {
                Id = 3,
                Nome = "Barba",
                IdCategoria = 1,
                IdEstabelecimento = 2
            }
        ];
    }

    private static IEnumerable<Categorium> ObterCategoriasDeTeste()
    {
        return
        [
            new Categorium { Id = 1, Nome = "Cabelo", IdEstabelecimento = 1 },
            new Categorium { Id = 2, Nome = "Unhas", IdEstabelecimento = 1 },
            new Categorium { Id = 3, Nome = "Outra", IdEstabelecimento = 2 }
        ];
    }
}
