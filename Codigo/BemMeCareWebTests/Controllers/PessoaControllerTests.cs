using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers.Tests;

[TestClass]
public class PessoaControllerTests
{
    private Mock<IPessoaService> pessoaServiceMock = null!;
    private PessoaController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        pessoaServiceMock = new Mock<IPessoaService>();

        pessoaServiceMock
            .Setup(service => service.GetAll())
            .Returns(ObterPessoasDeTeste());

        pessoaServiceMock
            .Setup(service => service.Get(1))
            .Returns(ObterPessoaAlvo());

        pessoaServiceMock
            .Setup(service => service.Get(999))
            .Returns((Pessoa?)null);

        controller = new PessoaController(pessoaServiceMock.Object);
    }


    [TestMethod]
    public void Index_QuandoChamado_DeveRetornaViewResultComListaDePessoas()
    {
        var resultado = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoas = Assert.IsInstanceOfType<List<PessoaViewModel>>(viewResult.ViewData.Model);

        Assert.HasCount(3, pessoas);

        var primeiraPessoa = pessoas.First();

        Assert.AreEqual(1U, primeiraPessoa.Id);
        Assert.AreEqual("João", primeiraPessoa.Nome);

        pessoaServiceMock.Verify(
            service => service.GetAll(), 
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemPessoas_DeveRetornaViewResultComListaVazia()
    {
        pessoaServiceMock
            .Setup(service => service.GetAll())
            .Returns(new List<Pessoa>());

        var resultado = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoas = Assert.IsInstanceOfType<List<PessoaViewModel>>(viewResult.ViewData.Model);

        Assert.IsEmpty(pessoas);

        pessoaServiceMock.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoPessoaExiste_DeveRetornaViewResultComPessoa()
    {
        var resultado = controller.Details(1);
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(viewResult.ViewData.Model);

        Assert.AreEqual(1U, pessoa.Id);
        Assert.AreEqual("João", pessoa.Nome);
        Assert.AreEqual(
            new DateTime(1839, 6, 21), 
            pessoa.DataNascimento);

        pessoaServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoPessoaNaoExiste_DeveRetornaViewComModeloVazio()
    {
        var resultado = controller.Details(999);

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
                viewResult.ViewData.Model);

        Assert.AreEqual(0U, pessoa.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoa.Nome));

        pessoaServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGet_DeveRetornaViewComModeloInicial()
    {
        var resultado = controller.Create();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
                viewResult.ViewData.Model);

        Assert.AreEqual(0U, pessoa.Id);
        Assert.IsFalse(pessoa.DataNascimento == default);
        Assert.IsTrue(pessoa.DataNascimento < DateTime.Now);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovaPessoa();
        Pessoa? pessoaCriada = null;

        pessoaServiceMock
            .Setup(service => service.Create(It.IsAny<Pessoa>()))
            .Callback<Pessoa>(pessoa => pessoaCriada = pessoa);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(pessoaCriada);
        Assert.AreEqual(modelo.Id, pessoaCriada.Id);
        Assert.AreEqual(modelo.Nome, pessoaCriada.Nome);
        Assert.AreEqual(
            modelo.DataNascimento,
            pessoaCriada.DataDeNascimento);

        pessoaServiceMock.Verify(
            service => service.Create(It.IsAny<Pessoa>()),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarPessoa()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(PessoaViewModel.Nome),
            "O nome é obrigatório.");

        // Act
        var resultado = controller.Create(CriarModeloDeNovaPessoa());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        pessoaServiceMock.Verify(
            service => service.Create(It.IsAny<Pessoa>()),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, pessoa.Id);
        Assert.AreEqual("Machado de Assis", pessoa.Nome);
        Assert.AreEqual(
            new DateTime(1839, 6, 21),
            pessoa.DataNascimento);

        pessoaServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEPessoaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, pessoa.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoa.Nome));

        pessoaServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDePessoaExistente();
        Pessoa? pessoaEditada = null;

        pessoaServiceMock
            .Setup(service => service.Edit(It.IsAny<Pessoa>()))
            .Callback<Pessoa>(pessoa => pessoaEditada = pessoa);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(pessoaEditada);
        Assert.AreEqual(modelo.Id, pessoaEditada.Id);
        Assert.AreEqual(modelo.Nome, pessoaEditada.Nome);
        Assert.AreEqual(
            modelo.DataNascimento,
            pessoaEditada.DataDeNascimento);

        pessoaServiceMock.Verify(
            service => service.Edit(It.IsAny<Pessoa>()),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarPessoa()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(PessoaViewModel.Nome),
            "O nome é obrigatório.");

        // Act
        var resultado = controller.Edit(CriarModeloDePessoaExistente());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        pessoaServiceMock.Verify(
            service => service.Edit(It.IsAny<Pessoa>()),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEPessoaExiste_DeveRetornarViewComPessoa()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, pessoa.Id);
        Assert.AreEqual("Machado de Assis", pessoa.Nome);
        Assert.AreEqual(
            new DateTime(1839, 6, 21),
            pessoa.DataNascimento);

        pessoaServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEPessoaNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var pessoa = Assert.IsInstanceOfType<PessoaViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, pessoa.Id);
        Assert.IsTrue(string.IsNullOrEmpty(pessoa.Nome));

        pessoaServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirPessoaERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDePessoaExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        pessoaServiceMock.Verify(
            service => service.Delete(modelo.Id),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(PessoaController.Index),
            redirectResult.ActionName);
    }

    private static PessoaViewModel CriarModeloDeNovaPessoa()
    {
        return new PessoaViewModel
        {
            Id = 4,
            Nome = "Ian Sommerville",
            Email = "iansommervile@email.com",
            Senha = "123456",
            Cpf = "123.456.789-00",
            Telefone = "(11) 98765-4321",
            DataNascimento = new DateTime(1950, 1, 1),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
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
            Id = 2,
            Nome = "Machado de Assis",
            Email = "machadoassis@email.com",
            Senha = "abcdef",
            Cpf = "987.654.321-00",
            Telefone = "(21) 12345-6789",
            DataNascimento = new DateTime(1839, 6, 21),
            Cor = "Parda",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Cep = "98765-432",
            Rua = "Rua B",
            Bairro = "Bairro C",
            Numero = "456",
            Complemento = "Casa",
            Cidade = "Cidade D",
            Estado = "Estado E"
        };
    }

    private static Pessoa ObterPessoaAlvo()
    {
        return new Pessoa
        {
            Id = 1,
            Nome = "Carla",
            Email = "carlasantos@email.com",
            Senha = "123456",
            Cpf = "123.456.789-00",
            Telefone = "(11) 98765-4321",
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = "Preto",
            Genero = "Feminino",
            Sexualidade = "Heterossexual",
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
        return
        [
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
                Cep = "98765-432",
                Rua = "Rua B",
                Bairro = "Bairro C",
                Numero = "456",
                Complemento = "Casa",
                Cidade = "Cidade D",
                Estado = "Estado E"
            }
        ];
    }
}

