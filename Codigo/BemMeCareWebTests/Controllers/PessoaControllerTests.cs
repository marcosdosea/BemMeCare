using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class PessoaControllerTests
{
    private PessoaController controller = null!;
    private Mock<IPessoaService> mockService = null!;

    [TestInitialize]
    public void Initialize()
    {
        mockService = new Mock<IPessoaService>();

        mockService.Setup(service => service.GetAll())
            .Returns(ObterPessoasDeTeste());
        mockService.Setup(service => service.Get(1))
            .Returns(ObterPessoaAlvo());
        mockService.Setup(service => service.Get(999))
            .Returns((Pessoa?)null);
        mockService.Setup(service => service.Edit(It.IsAny<Pessoa>()))
            .Verifiable();
        mockService.Setup(service => service.Create(It.IsAny<Pessoa>()))
            .Returns(4);
        mockService.Setup(service => service.Delete(It.IsAny<uint>()))
            .Verifiable();

        controller = new PessoaController(mockService.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComTodasPessoas()
    {
        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var listaPessoas = Assert.IsInstanceOfType<IEnumerable<PessoaViewModel>>(viewResult.ViewData.Model);

        Assert.HasCount(3, listaPessoas);

        AssertPessoaViewModel(CriarModeloDePessoaExistente(), listaPessoas.First());

        mockService.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemPessoas_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        mockService
            .Setup(service => service.GetAll())
            .Returns(Array.Empty<Pessoa>());

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var listaPessoas = Assert.IsInstanceOfType<IEnumerable<PessoaViewModel>>(viewResult.ViewData.Model);

        Assert.IsEmpty(listaPessoas);

        mockService.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);

        mockService.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoPessoaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var result = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        Assert.AreEqual(0U, pessoaModel.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoaModel.Nome));

        mockService.Verify(service => service.Get(999), Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornarView()
    {
        // Act
        var result = controller.Create();

        // Assert
        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovaPessoa();
        Pessoa? pessoaCriada = null;

        mockService
            .Setup(service => service.Create(It.IsAny<Pessoa>()))
            .Callback<Pessoa>(p => pessoaCriada = p);

        // Act
        var result = controller.Create(modelo);

        // Assert
        var redirectToActionResult = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);

        Assert.IsNotNull(pessoaCriada);
        Assert.AreEqual(modelo.Id, pessoaCriada.Id);
        Assert.AreEqual(modelo.Nome, pessoaCriada.Nome);
        Assert.AreEqual(modelo.Email, pessoaCriada.Email);
        Assert.AreEqual(modelo.Senha, pessoaCriada.Senha);
        Assert.AreEqual(modelo.DataDeNascimento, pessoaCriada.DataDeNascimento);
        Assert.AreEqual(modelo.Cor, pessoaCriada.Cor);
        Assert.AreEqual(modelo.Genero, pessoaCriada.Genero);
        Assert.AreEqual(modelo.Sexualidade, pessoaCriada.Sexualidade);
        Assert.AreEqual(modelo.Cep?.Replace("-", ""), pessoaCriada.Cep);
        Assert.AreEqual(modelo.Rua, pessoaCriada.Rua);
        Assert.AreEqual(modelo.Bairro, pessoaCriada.Bairro);
        Assert.AreEqual(modelo.Numero, pessoaCriada.Numero);
        Assert.AreEqual(modelo.Complemento, pessoaCriada.Complemento);
        Assert.AreEqual(modelo.Cidade, pessoaCriada.Cidade);
        Assert.AreEqual(modelo.Estado, pessoaCriada.Estado);

        mockService.Verify(
            service => service.Create(It.IsAny<Pessoa>()), 
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarPessoaERetornarView()
    {
        // Arrange
        var modelo = CriarModeloDeNovaPessoa();
        controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

        // Act
        var result = controller.Create(modelo);

        // Assert
        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        Assert.AreEqual(modelo, viewResult.ViewData.Model);

        mockService.Verify(
            service => service.Create(It.IsAny<Pessoa>()), 
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);

        mockService.Verify(
            service => service.Get(1), 
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEPessoaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var result = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        Assert.AreEqual(0U, pessoaModel.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoaModel.Nome));

        mockService.Verify(
            service => service.Get(999), 
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDePessoaExistente();
        Pessoa? pessoaEditada = null;

        mockService
            .Setup(service => service.Edit(It.IsAny<Pessoa>()))
            .Callback<Pessoa>(p => pessoaEditada = p);

        // Act
        var result = controller.Edit(modelo);

        // Assert
        var redirectToActionResult = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);

        Assert.IsNotNull(pessoaEditada);
        Assert.AreEqual(modelo.Id, pessoaEditada.Id);
        Assert.AreEqual(modelo.Nome, pessoaEditada.Nome);
        Assert.AreEqual(modelo.Email, pessoaEditada.Email);
        Assert.AreEqual(modelo.Senha, pessoaEditada.Senha);
        Assert.AreEqual(modelo.DataDeNascimento, pessoaEditada.DataDeNascimento);
        Assert.AreEqual(modelo.Cor, pessoaEditada.Cor);
        Assert.AreEqual(modelo.Genero, pessoaEditada.Genero);
        Assert.AreEqual(modelo.Sexualidade, pessoaEditada.Sexualidade);
        Assert.AreEqual(modelo.Cep?.Replace("-", ""), pessoaEditada.Cep);
        Assert.AreEqual(modelo.Rua, pessoaEditada.Rua);
        Assert.AreEqual(modelo.Bairro, pessoaEditada.Bairro);
        Assert.AreEqual(modelo.Numero, pessoaEditada.Numero);
        Assert.AreEqual(modelo.Complemento, pessoaEditada.Complemento);
        Assert.AreEqual(modelo.Cidade, pessoaEditada.Cidade);
        Assert.AreEqual(modelo.Estado, pessoaEditada.Estado);

        mockService.Verify(
            service => service.Edit(It.IsAny<Pessoa>()), 
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDePessoaExistente();
        controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

        // Act
        var result = controller.Edit(modelo);

        // Assert
        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        var redirectToActionResult = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);

        mockService.Verify(
            service => service.Edit(It.IsAny<Pessoa>()), 
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);

        mockService.Verify(
            service => service.Get(1), 
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEPessoaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var result = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pessoaModel = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        Assert.AreEqual(0U, pessoaModel.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoaModel.Nome));

        mockService.Verify(
            service => service.Get(999), 
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoPostEPessoaExiste_DeveExcluirPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDePessoaExistente();

        // Act
        var result = controller.Delete(modelo);

        // Assert
        var redirectToActionResult = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);

        mockService.Verify(
            service => service.Delete(modelo.Id), 
            Times.Once);
    }

    private static void AssertPessoaViewModel(PessoaViewModel esperado, PessoaViewModel obtido)
    {
        Assert.IsNotNull(esperado);
        Assert.IsNotNull(obtido);
        Assert.AreEqual(esperado.Id, obtido.Id);
        Assert.AreEqual(esperado.Nome, obtido.Nome);
        Assert.AreEqual(esperado.Email, obtido.Email);
        Assert.AreEqual(esperado.Senha, obtido.Senha);
        Assert.AreEqual(esperado.Cpf, obtido.Cpf);
        Assert.AreEqual(esperado.Telefone, obtido.Telefone);
        Assert.AreEqual(esperado.DataDeNascimento, obtido.DataDeNascimento);
        Assert.AreEqual(esperado.Cor, obtido.Cor);
        Assert.AreEqual(esperado.Genero, obtido.Genero);
        Assert.AreEqual(esperado.Sexualidade, obtido.Sexualidade);
        Assert.AreEqual(esperado.Foto, obtido.Foto);
        Assert.AreEqual(esperado.Cep, obtido.Cep);
        Assert.AreEqual(esperado.Rua, obtido.Rua);
        Assert.AreEqual(esperado.Bairro, obtido.Bairro);
        Assert.AreEqual(esperado.Numero, obtido.Numero);
        Assert.AreEqual(esperado.Complemento, obtido.Complemento);
        Assert.AreEqual(esperado.Cidade, obtido.Cidade);
        Assert.AreEqual(esperado.Estado, obtido.Estado);
    }

    private static PessoaViewModel CriarModeloDeNovaPessoa()
    {
        return new PessoaViewModel
        {
            Id = 1,
            Nome = "João",
            Email = "joaogomes@email.com",
            Senha = "123456",
            Cpf = "123.456.789-00",
            Telefone = "(11) 98765-4321",
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Foto = null,
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private static Pessoa ObterPessoaAlvo()
    {
        return new Pessoa
        {
            Id = 1,
            Nome = "João",
            Email = "joaogomes@email.com",
            Senha = "123456",
            Cpf = "123.456.789-00",
            Telefone = "(11) 98765-4321",
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Foto = null,
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private static PessoaViewModel CriarModeloDePessoaExistente()
    {
        return new PessoaViewModel
        {
            Id = 1,
            Nome = "João",
            Email = "joaogomes@email.com",
            Senha = "123456",
            Cpf = "123.456.789-00",
            Telefone = "(11) 98765-4321",
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Foto = null,
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private static IEnumerable<Pessoa> ObterPessoasDeTeste()
    {
        return new List<Pessoa>
        {
            new Pessoa
            {
                Id = 1,
                Nome = "João",
                Email = "joaogomes@email.com",
                Senha = "123456",
                Cpf = "123.456.789-00",
                Telefone = "(11) 98765-4321",
                DataDeNascimento = new DateTime(1990, 1, 1),
                Cor = "Branca",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Foto = null,
                Cep = "12345-678",
                Rua = "Rua A",
                Bairro = "Bairro B",
                Numero = "123",
                Complemento = "Apto 101",
                Cidade = "Cidade C",
                Estado = "Estado D"
            },
            new Pessoa
            {
                Id = 2,
                Nome = "Machado de Assis",
                Email = "machadoassis@email.com",
                Senha = "abcdef",
                Cpf = "987.654.321-00",
                Telefone = "(21) 12345-6789",
                DataDeNascimento = new DateTime(1839, 6, 21),
                Cor = "Parda",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Foto = null,
                Cep = "98765-432",
                Rua = "Rua B",
                Bairro = "Bairro C",
                Numero = "456",
                Complemento = "Casa",
                Cidade = "Cidade D",
                Estado = "Estado E"
            },
            new Pessoa
            {
                Id = 3,
                Nome = "Marcos Dósea",
                Email = "marcosdosea@email.com",
                Senha = "qwerty",
                Cpf = "111.222.333-44",
                Telefone = "(31) 98765-4321",
                DataDeNascimento = new DateTime(1982, 1, 1),
                Cor = "Parda",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Foto = null,
                Cep = "98765-432",
                Rua = "Rua B",
                Bairro = "Bairro C",
                Numero = "456",
                Complemento = "Casa",
                Cidade = "Cidade D",
                Estado = "Estado E"
            }
        };
    }
}
