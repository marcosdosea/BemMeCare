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
    private static PessoaController controller;
    private static Mock<IPessoaService> mockService;

    [TestInitialize]
    public void Initialize()
    {
        mockService = new Mock<IPessoaService>();

        mockService.Setup(service => service.GetAll())
            .Returns(ObterPessoasDeTeste());
        mockService.Setup(service => service.Get(1))
            .Returns(ObterPessoaAlvo());
        mockService.Setup(service => service.Edit(It.IsAny<Pessoa>()))
            .Verifiable();
        mockService.Setup(service => service.Create(It.IsAny<Pessoa>()))
            .Returns(4);
        mockService.Setup(service => service.Delete(It.IsAny<uint>()))
            .Verifiable();

        controller = new PessoaController(mockService.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComListaDePessoas()
    {
        // Act
        var result = controller.Index();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(List<PessoaViewModel>));

        List<PessoaViewModel> listaPessoas = (List<PessoaViewModel>)viewResult.ViewData.Model;
        Assert.AreEqual(3, listaPessoas.Count);
        AssertPessoaViewModel(CriarModeloDePessoaExistente(), listaPessoas[0]);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemPessoas_DeveRetornarViewComListaVazia()
    {
        // Arrange
        mockService.Setup(service => service.GetAll())
            .Returns(new List<Pessoa>());

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(List<PessoaViewModel>));

        List<PessoaViewModel> listaPessoas = (List<PessoaViewModel>)viewResult.ViewData.Model;
        Assert.AreEqual(0, listaPessoas.Count);
    }

    [TestMethod]
    public void Details_QuandoPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Details(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornarView()
    {
        // Act
        var result = controller.Create();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarPessoaERedirecionarParaIndex()
    {
        // Act
        var result = controller.Create(CriarModeloDeNovaPessoa());

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
        mockService.Verify(service => service.Create(It.IsAny<Pessoa>()), Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveChamarCreate()
    {
        // Arrange
        controller.ModelState.AddModelError("Nome", "Nome da pessoa é obrigatório");

        // Act
        var result = controller.Create(CriarModeloDeNovaPessoa());

        // Assert
        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
        mockService.Verify(service => service.Create(It.IsAny<Pessoa>()), Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Edit(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarPessoaERedirecionarParaIndex()
    {
        // Act
        var result = controller.Edit(CriarModeloDePessoaExistente());

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
        mockService.Verify(service => service.Edit(It.IsAny<Pessoa>()), Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveChamarEdit()
    {
        // Arrange
        controller.ModelState.AddModelError("Nome", "Nome da pessoa é obrigatório");

        // Act
        var result = controller.Edit(CriarModeloDePessoaExistente());

        // Assert
        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
        mockService.Verify(service => service.Edit(It.IsAny<Pessoa>()), Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var result = controller.Delete(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        AssertPessoaViewModel(CriarModeloDePessoaExistente(), pessoaModel);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirPessoaERedirecionarParaIndex()
    {
        // Act
        var result = controller.Delete(CriarModeloDePessoaExistente());

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
        mockService.Verify(service => service.Delete(It.IsAny<uint>()), Times.Once);
    }

    private static void AssertPessoaViewModel(PessoaViewModel esperado, PessoaViewModel obtido)
    {
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
