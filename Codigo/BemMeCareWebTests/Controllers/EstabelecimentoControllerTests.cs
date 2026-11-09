using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class EstabelecimentoControllerTests
{
    private Mock<IEstabelecimentoService> estabelecimentoServiceMock = null!;
    private EstabelecimentoController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        estabelecimentoServiceMock = new Mock<IEstabelecimentoService>();

        estabelecimentoServiceMock
            .Setup(service => service.GetAll())
            .Returns(ObterEstabelecimentosDeTeste());

        estabelecimentoServiceMock
            .Setup(service => service.Get(1))
            .Returns(ObterEstabelecimentoAlvo());

        estabelecimentoServiceMock
            .Setup(service => service.Get(999))
            .Returns((Estabelecimento?)null);

        controller = new EstabelecimentoController(estabelecimentoServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComTodosOsEstabelecimentos()
    {
        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimentos = Assert.IsInstanceOfType<IEnumerable<EstabelecimentoViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(3, estabelecimentos);

        var primeiro = estabelecimentos.First();

        Assert.AreEqual(1u, primeiro.Id);
        Assert.AreEqual("Salão Espaço Bella", primeiro.Nome);
        Assert.AreEqual("Modalidade-1", primeiro.Modalidade);
        Assert.AreEqual("12345678000190", primeiro.Cnpj);
        Assert.AreEqual("49010000", primeiro.Cep);
        Assert.AreEqual("Rua Laranjeiras", primeiro.Rua);
        Assert.AreEqual("245", primeiro.Numero);
        Assert.AreEqual("Loja 2", primeiro.Complemento);
        Assert.AreEqual("Centro", primeiro.Bairro);
        Assert.AreEqual("Aracaju", primeiro.Cidade);
        Assert.AreEqual("Sergipe", primeiro.Estado);
        Assert.AreEqual("(79) 3211-4455", primeiro.Contato);
        Assert.AreEqual("contato@espacobella.com.br", primeiro.ChavePix);
        Assert.AreEqual(1u, primeiro.IdPessoa);

        estabelecimentoServiceMock.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemEstabelecimentos_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        estabelecimentoServiceMock
            .Setup(service => service.GetAll())
            .Returns(Array.Empty<Estabelecimento>());

        // Act
        var resultado = controller.Index();

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimentos = Assert.IsInstanceOfType<IEnumerable<EstabelecimentoViewModel>>(
            viewResult.ViewData.Model);

        Assert.IsEmpty(estabelecimentos);

        estabelecimentoServiceMock.Verify(
            service => service.GetAll(),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoEstabelecimentoExiste_DeveRetornarViewComEstabelecimento()
    {
        // Act
        var resultado = controller.Details(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, estabelecimento.Id);
        Assert.AreEqual("Barbearia Navalha de Ouro", estabelecimento.Nome);
        Assert.AreEqual("Modalidade-5", estabelecimento.Modalidade);
        Assert.AreEqual("98765432000110", estabelecimento.Cnpj);
        Assert.AreEqual("49015120", estabelecimento.Cep);
        Assert.AreEqual("Rua Alberto", estabelecimento.Rua);
        Assert.AreEqual("456", estabelecimento.Numero);
        Assert.AreEqual("Sala 3", estabelecimento.Complemento);
        Assert.AreEqual("São José", estabelecimento.Bairro);
        Assert.AreEqual("Aracaju", estabelecimento.Cidade);
        Assert.AreEqual("Sergipe", estabelecimento.Estado);
        Assert.AreEqual("(79) 99876-5432", estabelecimento.Contato);
        Assert.AreEqual("contato@navalhadeouro.com.br", estabelecimento.ChavePix);
        Assert.AreEqual(2u, estabelecimento.IdPessoa);

        estabelecimentoServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoEstabelecimentoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, estabelecimento.Id);
        Assert.IsTrue(string.IsNullOrEmpty(estabelecimento.Nome));

        estabelecimentoServiceMock.Verify(
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
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, estabelecimento.Id);
        Assert.IsTrue(string.IsNullOrEmpty(estabelecimento.Nome));
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveCriarEstabelecimentoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovoEstabelecimento();
        Estabelecimento? estabelecimentoCriado = null;

        estabelecimentoServiceMock
            .Setup(service => service.Create(It.IsAny<Estabelecimento>()))
            .Callback<Estabelecimento>(estabelecimento => estabelecimentoCriado = estabelecimento);

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(estabelecimentoCriado);
        Assert.AreEqual(modelo.Nome, estabelecimentoCriado.Nome);
        Assert.AreEqual(modelo.Id, estabelecimentoCriado.Id);
        Assert.AreEqual(modelo.Modalidade, estabelecimentoCriado.Modalidade);
        Assert.AreEqual(modelo.Cnpj, estabelecimentoCriado.Cnpj);
        Assert.AreEqual(modelo.Cep, estabelecimentoCriado.Cep);
        Assert.AreEqual(modelo.Rua, estabelecimentoCriado.Rua);
        Assert.AreEqual(modelo.Numero, estabelecimentoCriado.Numero);
        Assert.AreEqual(modelo.Complemento, estabelecimentoCriado.Complemento);
        Assert.AreEqual(modelo.Bairro, estabelecimentoCriado.Bairro);
        Assert.AreEqual(modelo.Cidade, estabelecimentoCriado.Cidade);
        Assert.AreEqual(modelo.Estado, estabelecimentoCriado.Estado);
        Assert.AreEqual(modelo.Contato, estabelecimentoCriado.Contato);
        Assert.AreEqual(modelo.ChavePix, estabelecimentoCriado.ChavePix);
        Assert.AreEqual(modelo.Contato, estabelecimentoCriado.Contato);
        Assert.AreEqual(modelo.IdPessoa, estabelecimentoCriado.IdPessoa);


        estabelecimentoServiceMock.Verify(
            service => service.Create(It.IsAny<Estabelecimento>()),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveCriarEstabelecimento()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(EstabelecimentoViewModel.Nome),
            "O nome é obrigatório.");

        // Act
        var resultado = controller.Create(CriarModeloDeNovoEstabelecimento());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        estabelecimentoServiceMock.Verify(
            service => service.Create(It.IsAny<Estabelecimento>()),
            Times.Never);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEEstabelecimentoExiste_DeveRetornarViewComEstabelecimento()
    {
        // Act
        var resultado = controller.Edit(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, estabelecimento.Id);
        Assert.AreEqual("Barbearia Navalha de Ouro", estabelecimento.Nome);
        Assert.AreEqual("Modalidade-5", estabelecimento.Modalidade);
        Assert.AreEqual("98765432000110", estabelecimento.Cnpj);
        Assert.AreEqual("Aracaju", estabelecimento.Cidade);
        Assert.AreEqual(2u, estabelecimento.IdPessoa);
        Assert.AreEqual("49015120", estabelecimento.Cep);
        Assert.AreEqual("Rua Alberto", estabelecimento.Rua);
        Assert.AreEqual("456", estabelecimento.Numero);
        Assert.AreEqual("Sala 3", estabelecimento.Complemento);
        Assert.AreEqual("São José", estabelecimento.Bairro);
        Assert.AreEqual("Sergipe", estabelecimento.Estado);
        Assert.AreEqual("(79) 99876-5432", estabelecimento.Contato);
        Assert.AreEqual("contato@navalhadeouro.com.br", estabelecimento.ChavePix);

        estabelecimentoServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoRequisicaoGetEEstabelecimentoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Edit(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, estabelecimento.Id);
        Assert.IsTrue(string.IsNullOrEmpty(estabelecimento.Nome));

        estabelecimentoServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloValido_DeveAtualizarEstabelecimentoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeEstabelecimentoExistente();
        Estabelecimento? estabelecimentoEditado = null;

        estabelecimentoServiceMock
            .Setup(service => service.Edit(It.IsAny<Estabelecimento>()))
            .Callback<Estabelecimento>(estabelecimento => estabelecimentoEditado = estabelecimento);

        // Act
        var resultado = controller.Edit(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        Assert.IsNotNull(estabelecimentoEditado);
        Assert.AreEqual(modelo.Id, estabelecimentoEditado.Id);
        Assert.AreEqual(modelo.Nome, estabelecimentoEditado.Nome);
        Assert.AreEqual(modelo.Modalidade, estabelecimentoEditado.Modalidade);
        Assert.AreEqual(modelo.Cnpj, estabelecimentoEditado.Cnpj);
        Assert.AreEqual(modelo.Cep, estabelecimentoEditado.Cep);
        Assert.AreEqual(modelo.Rua, estabelecimentoEditado.Rua);
        Assert.AreEqual(modelo.Numero, estabelecimentoEditado.Numero);
        Assert.AreEqual(modelo.Complemento, estabelecimentoEditado.Complemento);
        Assert.AreEqual(modelo.Bairro, estabelecimentoEditado.Bairro);
        Assert.AreEqual(modelo.Cidade, estabelecimentoEditado.Cidade);
        Assert.AreEqual(modelo.Estado, estabelecimentoEditado.Estado);
        Assert.AreEqual(modelo.Contato, estabelecimentoEditado.Contato);
        Assert.AreEqual(modelo.ChavePix, estabelecimentoEditado.ChavePix);
        Assert.AreEqual(modelo.IdPessoa, estabelecimentoEditado.IdPessoa);

        estabelecimentoServiceMock.Verify(
            service => service.Edit(It.IsAny<Estabelecimento>()),
            Times.Once);
    }

    [TestMethod]
    public void Edit_QuandoModeloInvalido_NaoDeveAtualizarEstabelecimento()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(EstabelecimentoViewModel.Nome),
            "O nome é obrigatório.");

        // Act
        var resultado = controller.Edit(CriarModeloDeEstabelecimentoExistente());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        estabelecimentoServiceMock.Verify(
            service => service.Edit(It.IsAny<Estabelecimento>()),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEEstabelecimentoExiste_DeveRetornarViewComEstabelecimento()
    {
        // Act
        var resultado = controller.Delete(1);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(1u, estabelecimento.Id);
        Assert.AreEqual("Barbearia Navalha de Ouro", estabelecimento.Nome);
        Assert.AreEqual("Modalidade-5", estabelecimento.Modalidade);
        Assert.AreEqual("98765432000110", estabelecimento.Cnpj);
        Assert.AreEqual("49015120", estabelecimento.Cep);
        Assert.AreEqual("Rua Alberto", estabelecimento.Rua);
        Assert.AreEqual("456", estabelecimento.Numero);
        Assert.AreEqual("Sala 3", estabelecimento.Complemento);
        Assert.AreEqual("São José", estabelecimento.Bairro);
        Assert.AreEqual("Aracaju", estabelecimento.Cidade);
        Assert.AreEqual("Sergipe", estabelecimento.Estado);
        Assert.AreEqual("(79) 99876-5432", estabelecimento.Contato);
        Assert.AreEqual("contato@navalhadeouro.com.br", estabelecimento.ChavePix);
        Assert.AreEqual(2u, estabelecimento.IdPessoa);

        estabelecimentoServiceMock.Verify(
            service => service.Get(1),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEEstabelecimentoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var estabelecimento = Assert.IsInstanceOfType<EstabelecimentoViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, estabelecimento.Id);
        Assert.IsTrue(string.IsNullOrEmpty(estabelecimento.Nome));

        estabelecimentoServiceMock.Verify(
            service => service.Get(999),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveExcluirEstabelecimentoERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeEstabelecimentoExistente();

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado);

        estabelecimentoServiceMock.Verify(
            service => service.Delete(modelo.Id),
            Times.Once);
    }

    private static void VerificarRedirecionamentoParaIndex(ActionResult resultado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(EstabelecimentoController.Index),
            redirectResult.ActionName);
    }

    private static EstabelecimentoViewModel CriarModeloDeNovoEstabelecimento()
    {
        return new EstabelecimentoViewModel
        {
            Id = 5,
            Nome = "Estabelecimento de Teste",
            Modalidade = "modalidade-de-teste",
            Cnpj = "12345678000190",
            Cep = "12345678",
            Rua = "Rua de Teste",
            Numero = "123",
            Complemento = "Complemento de Teste",
            Bairro = "Bairro de Teste",
            Cidade = "Cidade de Teste",
            Estado = "Estado de Teste",
            Contato = "(79) 91234-5678",
            ChavePix = "chave-pix-de-teste",
            IdPessoa = 1
        };
    }

    private static EstabelecimentoViewModel CriarModeloDeEstabelecimentoExistente()
    {
        return new EstabelecimentoViewModel
        {
            Id = 1,
            Nome = "Mercado Bom Preço",
            Modalidade = "Modalidade Existente",
            Cnpj = "11222333000181",
            Cep = "49010000",
            Rua = "Rua Laranjeiras",
            Numero = "245",
            Complemento = "Complemento Existente",
            Bairro = "Bairro Existente",
            Cidade = "Cidade Existente",
            Estado = "Estado Existente",
            Contato = "(79) 99876-5432",
            ChavePix = "chave-pix-existente",
            IdPessoa = 2
        };
    }

    private static Estabelecimento ObterEstabelecimentoAlvo()
    {
        return new Estabelecimento
        {
            Id = 1,
            Nome = "Barbearia Navalha de Ouro",
            Modalidade = "Modalidade-5",
            Cnpj = "98765432000110",
            Cep = "49015120",
            Rua = "Rua Alberto",
            Numero = "456",
            Complemento = "Sala 3",
            Bairro = "São José",
            Cidade = "Aracaju",
            Estado = "Sergipe",
            Contato = "(79) 99876-5432",
            ChavePix = "contato@navalhadeouro.com.br",
            IdPessoa = 2
        };
    }

    private static IEnumerable<Estabelecimento> ObterEstabelecimentosDeTeste()
    {
        return
        [
            new Estabelecimento
            {
                Id = 1,
                Nome = "Salão Espaço Bella",
                Modalidade = "Modalidade-1",
                Cnpj = "12345678000190",
                Cep = "49010000",
                Rua = "Rua Laranjeiras",
                Numero = "245",
                Complemento = "Loja 2",
                Bairro = "Centro",
                Cidade = "Aracaju",
                Estado = "Sergipe",
                Contato = "(79) 3211-4455",
                ChavePix = "contato@espacobella.com.br",
                IdPessoa = 1
            },
            new Estabelecimento
            {
                Id = 2,
                Nome = "Barbearia Navalha de Ouro",
                Modalidade = "Modalidade-2",
                Cnpj = "98765432000110",
                Cep = "40015970",
                Rua = "Avenida Sete de Setembro",
                Numero = "1500",
                Complemento = "Sala 304",
                Bairro = "Barra",
                Cidade = "Salvador",
                Estado = "Bahia",
                Contato = "(71) 99876-5432",
                ChavePix = "11222333000181",
                IdPessoa = 2
            },
            new Estabelecimento
            {
                Id = 3,
                Nome = "Studio Glamour Nails",
                Modalidade = "Modalidade-3",
                Cnpj = "11222333000181",
                Cep = "01310100",
                Rua = "Avenida Paulista",
                Numero = "900",
                Complemento = "Conjunto 12",
                Bairro = "Bela Vista",
                Cidade = "São Paulo",
                Estado = "SP",
                Contato = "(11) 91234-0000",
                ChavePix = "+5511912340000",
                IdPessoa = 3
            }
        ];
    }
}