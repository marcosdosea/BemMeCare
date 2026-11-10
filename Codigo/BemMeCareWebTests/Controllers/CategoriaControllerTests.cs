using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class CategoriaControllerTests
{
    // Mesmo valor da constante provisória do controller.
    private const uint idEstabelecimentoEsperado = 1;

    private Mock<ICategoriaService> categoriaServiceMock = null!;
    private CategoriaController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        categoriaServiceMock = new Mock<ICategoriaService>();

        categoriaServiceMock
            .Setup(service => service.GetAll(idEstabelecimentoEsperado))
            .Returns(ObterCategoriasDeTeste());

        categoriaServiceMock
            .Setup(service => service.Get(1, idEstabelecimentoEsperado))
            .Returns(ObterCategoriaAlvo());

        categoriaServiceMock
            .Setup(service => service.Get(999, idEstabelecimentoEsperado))
            .Returns((Categorium?)null);

        controller = new CategoriaController(categoriaServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComTodasAsCategoriasDoEstabelecimento()
    {
        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categorias = Assert.IsInstanceOfType<IEnumerable<CategoriaViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(3, categorias);

        var primeiraCategoria = categorias.First();

        Assert.AreEqual(1u, primeiraCategoria.Id);
        Assert.AreEqual("Cabeleireiro", primeiraCategoria.Nome);

        categoriaServiceMock.Verify(
            service => service.GetAll(idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemCategorias_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        categoriaServiceMock
            .Setup(service => service.GetAll(idEstabelecimentoEsperado))
            .Returns(Array.Empty<Categorium>());

        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categorias = Assert.IsInstanceOfType<IEnumerable<CategoriaViewModel>>(
            viewResult.ViewData.Model);

        Assert.IsEmpty(categorias);

        categoriaServiceMock.Verify(
            service => service.GetAll(idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoCategoriaExiste_DeveRetornarViewComCategoria()
    {
        // Act
        var resultado = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, categoria.Id);
        Assert.AreEqual("Cabeleireiro", categoria.Nome);
        Assert.AreEqual(idEstabelecimentoEsperado, categoria.IdEstabelecimento);

        categoriaServiceMock.Verify(
            service => service.Get(1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoCategoriaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, categoria.Id);
        Assert.IsTrue(string.IsNullOrEmpty(categoria.Nome));

        categoriaServiceMock.Verify(
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
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, categoria.Id);
        Assert.IsTrue(string.IsNullOrEmpty(categoria.Nome));
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarCategoriaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovaCategoria();
        Categorium? categoriaCriada = null;

        categoriaServiceMock
            .Setup(service => service.Create(It.IsAny<Categorium>()))
            .Callback<Categorium>(categoria => categoriaCriada = categoria)
            .Returns(4u);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(categoriaCriada);
        Assert.AreEqual(modelo.Nome, categoriaCriada.Nome);
        // O estabelecimento vem do servidor, não do model enviado pela tela
        Assert.AreEqual(idEstabelecimentoEsperado, categoriaCriada.IdEstabelecimento);

        categoriaServiceMock.Verify(
            service => service.Create(It.IsAny<Categorium>()),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarCategoria()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(CategoriaViewModel.Nome),
            "Nome da categoria é obrigatório");

        // Act
        var resultado = controller.Create(CriarModeloDeNovaCategoria());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        categoriaServiceMock.Verify(
            service => service.Create(It.IsAny<Categorium>()),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetECategoriaExiste_DeveRetornarViewComCategoria()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, categoria.Id);
        Assert.AreEqual("Cabeleireiro", categoria.Nome);
        Assert.AreEqual(idEstabelecimentoEsperado, categoria.IdEstabelecimento);

        categoriaServiceMock.Verify(
            service => service.Get(1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetECategoriaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, categoria.Id);
        Assert.IsTrue(string.IsNullOrEmpty(categoria.Nome));

        categoriaServiceMock.Verify(
            service => service.Get(999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarCategoriaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeCategoriaExistente();
        Categorium? categoriaEditada = null;

        categoriaServiceMock
            .Setup(service => service.Edit(It.IsAny<Categorium>()))
            .Callback<Categorium>(categoria => categoriaEditada = categoria);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(categoriaEditada);
        Assert.AreEqual(modelo.Id, categoriaEditada.Id);
        Assert.AreEqual(modelo.Nome, categoriaEditada.Nome);
        Assert.AreEqual(idEstabelecimentoEsperado, categoriaEditada.IdEstabelecimento);

        categoriaServiceMock.Verify(
            service => service.Edit(It.IsAny<Categorium>()),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarCategoria()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(CategoriaViewModel.Nome),
            "Nome da categoria é obrigatório");

        // Act
        var resultado = controller.Edit(CriarModeloDeCategoriaExistente());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        categoriaServiceMock.Verify(
            service => service.Edit(It.IsAny<Categorium>()),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetECategoriaExiste_DeveRetornarViewComCategoria()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, categoria.Id);
        Assert.AreEqual("Cabeleireiro", categoria.Nome);
        Assert.AreEqual(idEstabelecimentoEsperado, categoria.IdEstabelecimento);

        categoriaServiceMock.Verify(
            service => service.Get(1, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetECategoriaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var categoria = Assert.IsInstanceOfType<CategoriaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, categoria.Id);
        Assert.IsTrue(string.IsNullOrEmpty(categoria.Nome));

        categoriaServiceMock.Verify(
            service => service.Get(999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirCategoriaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeCategoriaExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        categoriaServiceMock.Verify(
            service => service.Delete(modelo.Id, idEstabelecimentoEsperado),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(CategoriaController.Index),
            redirectResult.ActionName);
    }

    private static CategoriaViewModel CriarModeloDeNovaCategoria()
    {
        return new CategoriaViewModel
        {
            Nome = "Esteticista"
        };
    }

    private static CategoriaViewModel CriarModeloDeCategoriaExistente()
    {
        return new CategoriaViewModel
        {
            Id = 1,
            Nome = "Cabeleireiro"
        };
    }

    private static Categorium ObterCategoriaAlvo()
    {
        return new Categorium
        {
            Id = 1,
            Nome = "Cabeleireiro",
            IdEstabelecimento = idEstabelecimentoEsperado
        };
    }

    private static IEnumerable<Categorium> ObterCategoriasDeTeste()
    {
        return
        [
            new Categorium
            {
                Id = 1,
                Nome = "Cabeleireiro",
                IdEstabelecimento = idEstabelecimentoEsperado
            },
            new Categorium
            {
                Id = 2,
                Nome = "Manicure",
                IdEstabelecimento = idEstabelecimentoEsperado
            },
            new Categorium
            {
                Id = 3,
                Nome = "Barbeiro",
                IdEstabelecimento = idEstabelecimentoEsperado
            }
        ];
    }
}