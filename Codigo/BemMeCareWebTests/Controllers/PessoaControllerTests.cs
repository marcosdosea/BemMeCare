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

    [TestInitialize]
    public void Initialize()
    {
        var mockService = new Mock<IPessoaService>();

        mockService.Setup(service => service.GetAll())
            .Returns(GetTestPessoas());
        mockService.Setup(service => service.Get(1))
            .Returns(GetTargetPessoa());
        mockService.Setup(service => service.Edit(It.IsAny<Pessoa>()))
            .Verifiable();
        mockService.Setup(service => service.Create(It.IsAny<Pessoa>()))
            .Returns(4);
        mockService.Setup(service => service.Delete(It.IsAny<uint>()))
            .Verifiable();
        controller = new PessoaController(mockService.Object);
    }

    [TestMethod]
    public void IndexTest_Valido()
    {
        var result = controller.Index();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(List<PessoaViewModel>));

        List<PessoaViewModel> listaPessoas = (List<PessoaViewModel>)viewResult.ViewData.Model;
        Assert.AreEqual(3, listaPessoas.Count);
    }

    [TestMethod]
    public void DetailsTest_Valido()
    {
        var result = controller.Details(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        Assert.AreEqual(1U, pessoaModel.Id);
        Assert.AreEqual("João", pessoaModel.Nome);
        Assert.AreEqual("joaogomes@email.com", pessoaModel.Email);
    }

    [TestMethod]
    public void CreateTest_Get_Valido()
    {
        var result = controller.Create();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void CreateTest_Post_Valido()
    {
        var result = controller.Create(GetNewPessoa());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void CreateTest_Post_Invalido()
    {
        controller.ModelState.AddModelError("Nome", "Nome da pessoa é obrigatório");

        var result = controller.Create(GetNewPessoa());

        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void EditTest_Get_Valido()
    {
        var result = controller.Edit(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        Assert.AreEqual(1U, pessoaModel.Id);
        Assert.AreEqual("João", pessoaModel.Nome);
        Assert.AreEqual("joaogomes@email.com", pessoaModel.Email);
    }

    [TestMethod]
    public void EditTest_Post_Valido()
    {
        var result = controller.Edit(GetTargetPessoaModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void DeleteTest_Get_Valido()
    {
        var result = controller.Delete(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(PessoaViewModel));
        PessoaViewModel pessoaModel = (PessoaViewModel)viewResult.ViewData.Model;
        Assert.AreEqual(1U, pessoaModel.Id);
        Assert.AreEqual("João", pessoaModel.Nome);
        Assert.AreEqual("joaogomes@email.com", pessoaModel.Email);
    }

    [TestMethod]
    public void DeleteTest_Post_Valido()
    {
        var result = controller.Delete(GetTargetPessoaModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult)result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    private PessoaViewModel GetNewPessoa()
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
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private Pessoa GetTargetPessoa()
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
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private PessoaViewModel GetTargetPessoaModel()
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
            Cep = "12345-678",
            Rua = "Rua A",
            Bairro = "Bairro B",
            Numero = "123",
            Complemento = "Apto 101",
            Cidade = "Cidade C",
            Estado = "Estado D"
        };
    }

    private IEnumerable<Pessoa> GetTestPessoas()
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
        };
    }
}
