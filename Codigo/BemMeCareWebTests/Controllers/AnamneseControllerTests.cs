using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class AnamneseControllerTests
{
    private const uint idEstabelecimentoEsperado = 1;
    private const uint idProfissional = 1;

    private Mock<IProfissionalService> profissionalServiceMock = null!;
    private AnamneseController controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        profissionalServiceMock = new Mock<IProfissionalService>();

        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, idEstabelecimentoEsperado))
            .Returns(ObterServicosDeTeste());

        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, 3, idEstabelecimentoEsperado))
            .Returns(ObterServicoAlvo());

        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, 4, idEstabelecimentoEsperado))
            .Returns(ObterServicoSemAnamnese());

        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, 5, idEstabelecimentoEsperado))
            .Returns(ObterServicoComLinkDesabilitado());

        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, 999, idEstabelecimentoEsperado))
            .Returns((Servicoprofissional?)null);

        controller = new AnamneseController(profissionalServiceMock.Object);
    }

    [TestMethod]
    public void Index_QuandoChamado_DeveRetornarViewComTodosOsServicosDoProfissional()
    {
        // Act
        var resultado = controller.Index(idProfissional);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamneses = Assert.IsInstanceOfType<IEnumerable<AnamneseViewModel>>(
            viewResult.ViewData.Model);

        Assert.HasCount(2, anamneses);

        var primeiraAnamnese = anamneses.First();

        Assert.AreEqual(idProfissional, primeiraAnamnese.IdProfissional);
        Assert.AreEqual(3u, primeiraAnamnese.IdServico);
        Assert.AreEqual("https://forms.gle/abc123", primeiraAnamnese.FormAnamnese);

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Index_QuandoNaoExistemServicos_DeveRetornarViewComColecaoVazia()
    {
        // Arrange
        profissionalServiceMock
            .Setup(service => service.GetServicoProfissional(idProfissional, idEstabelecimentoEsperado))
            .Returns(Array.Empty<Servicoprofissional>());

        // Act
        var resultado = controller.Index(idProfissional);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamneses = Assert.IsInstanceOfType<IEnumerable<AnamneseViewModel>>(
            viewResult.ViewData.Model);

        Assert.IsEmpty(anamneses);

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoServicoExiste_DeveRetornarViewComAnamnese()
    {
        // Act
        var resultado = controller.Details(idProfissional, 3);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(idProfissional, anamnese.IdProfissional);
        Assert.AreEqual(3u, anamnese.IdServico);
        Assert.AreEqual("https://forms.gle/abc123", anamnese.FormAnamnese);
        Assert.AreEqual(idEstabelecimentoEsperado, anamnese.IdEstabelecimento);

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 3, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Details_QuandoServicoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Details(idProfissional, 999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, anamnese.IdServico);
        Assert.IsTrue(string.IsNullOrEmpty(anamnese.FormAnamnese));

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGetEServicoJaTemAnamnese_DeveRetornarViewComLinkAtual()
    {
        // Act
        var resultado = controller.Create(idProfissional, 3);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(idProfissional, anamnese.IdProfissional);
        Assert.AreEqual(3u, anamnese.IdServico);
        Assert.AreEqual("https://forms.gle/abc123", anamnese.FormAnamnese);
        Assert.AreEqual(idEstabelecimentoEsperado, anamnese.IdEstabelecimento);

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 3, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoRequisicaoGetEServicoNaoExiste_DeveRetornarViewComModeloInicial()
    {
        // Act
        var resultado = controller.Create(idProfissional, 999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(idProfissional, anamnese.IdProfissional);
        Assert.AreEqual(999u, anamnese.IdServico);
        Assert.IsTrue(string.IsNullOrEmpty(anamnese.FormAnamnese));

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloValido_DeveGravarAnamneseERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeNovaAnamnese();
        modelo.ExigeAnamnese = false; // quem liga a flag é o controller, ao cadastrar o link
        uint? idProfissionalEnviado = null;
        List<Servicoprofissional>? servicosEnviados = null;

        profissionalServiceMock
            .Setup(service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()))
            .Callback<uint, IEnumerable<Servicoprofissional>>((id, servicos) =>
            {
                idProfissionalEnviado = id;
                servicosEnviados = servicos.ToList();
            });

        // Act
        var resultado = controller.Create(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, modelo.IdProfissional);

        Assert.AreEqual(modelo.IdProfissional, idProfissionalEnviado);
        Assert.IsNotNull(servicosEnviados);
        Assert.HasCount(1, servicosEnviados);

        var servico = servicosEnviados[0];
        Assert.AreEqual(modelo.IdServico, servico.IdServico);
        Assert.AreEqual(modelo.FormAnamnese, servico.FormAnamnese);
        Assert.AreEqual(idEstabelecimentoEsperado, servico.IdEstabelecimento);
        Assert.AreEqual<sbyte>(1, servico.ExigeAnamnese); // cadastrar o link ativa a anamnese

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                idProfissional,
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Once);
    }

    [TestMethod]
    public void Create_QuandoModeloInvalido_NaoDeveGravarAnamnese()
    {
        // Arrange
        controller.ModelState.AddModelError(
            nameof(AnamneseViewModel.FormAnamnese),
            "O link do formulário é obrigatório");

        // Act
        var resultado = controller.Create(CriarModeloDeNovaAnamnese());

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, idProfissional);

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Never);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEServicoExiste_DeveRetornarViewComAnamnese()
    {
        // Act
        var resultado = controller.Delete(idProfissional, 3);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(idProfissional, anamnese.IdProfissional);
        Assert.AreEqual(3u, anamnese.IdServico);
        Assert.AreEqual("https://forms.gle/abc123", anamnese.FormAnamnese);
        Assert.AreEqual(idEstabelecimentoEsperado, anamnese.IdEstabelecimento);

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 3, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoGetEServicoNaoExiste_DeveRetornarViewComModeloVazio()
    {
        // Act
        var resultado = controller.Delete(idProfissional, 999);

        // Assert
        var viewResult = Assert.IsInstanceOfType<ViewResult>(resultado);
        var anamnese = Assert.IsInstanceOfType<AnamneseViewModel>(
            viewResult.ViewData.Model);

        Assert.AreEqual(0u, anamnese.IdServico);
        Assert.IsTrue(string.IsNullOrEmpty(anamnese.FormAnamnese));

        profissionalServiceMock.Verify(
            service => service.GetServicoProfissional(idProfissional, 999, idEstabelecimentoEsperado),
            Times.Once);
    }

    [TestMethod]
    public void Delete_QuandoRequisicaoPost_DeveApagarSoOLinkERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeAnamneseExistente();
        List<Servicoprofissional>? servicosEnviados = null;

        profissionalServiceMock
            .Setup(service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()))
            .Callback<uint, IEnumerable<Servicoprofissional>>((_, servicos) =>
                servicosEnviados = servicos.ToList());

        // Act
        var resultado = controller.Delete(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, modelo.IdProfissional);

        Assert.IsNotNull(servicosEnviados);
        Assert.HasCount(1, servicosEnviados);
        Assert.AreEqual(modelo.IdServico, servicosEnviados[0].IdServico);
        Assert.AreEqual(string.Empty, servicosEnviados[0].FormAnamnese);
        Assert.AreEqual<sbyte>(0, servicosEnviados[0].ExigeAnamnese);
        Assert.AreEqual(idEstabelecimentoEsperado, servicosEnviados[0].IdEstabelecimento);

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                idProfissional,
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Once);

        profissionalServiceMock.Verify(
            service => service.Delete(It.IsAny<uint>()),
            Times.Never);
    }

    [TestMethod]
    public void AlterarExigencia_QuandoMarcaServicoComLink_DeveLigarAFlagERedirecionarParaIndex()
    {
        // Arrange (serviço 5: tem link, mas a flag está desligada)
        var modelo = CriarModeloDeAlteracaoDeExigencia(idServico: 5, exigeAnamnese: true);
        List<Servicoprofissional>? servicosEnviados = null;

        profissionalServiceMock
            .Setup(service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()))
            .Callback<uint, IEnumerable<Servicoprofissional>>((_, servicos) =>
                servicosEnviados = servicos.ToList());

        // Act
        var resultado = controller.AlterarExigencia(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, idProfissional);

        Assert.IsNotNull(servicosEnviados);
        Assert.AreEqual<sbyte>(1, servicosEnviados.Single().ExigeAnamnese);
        Assert.AreEqual("https://forms.gle/guardado", servicosEnviados.Single().FormAnamnese);

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                idProfissional,
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Once);
    }

    [TestMethod]
    public void AlterarExigencia_QuandoMarcaServicoSemLink_NaoDeveGravarERedirecionarParaCreate()
    {
        // Arrange (serviço 4: link vazio)
        var modelo = CriarModeloDeAlteracaoDeExigencia(idServico: 4, exigeAnamnese: true);

        // Act
        var resultado = controller.AlterarExigencia(modelo);

        // Assert
        var redirectResult = Assert.IsInstanceOfType<RedirectToActionResult>(resultado);
        Assert.AreEqual(nameof(AnamneseController.Create), redirectResult.ActionName);
        Assert.AreEqual(idProfissional, redirectResult.RouteValues!["idProfissional"]);
        Assert.AreEqual(4u, redirectResult.RouteValues!["idServico"]);

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Never);
    }

    [TestMethod]
    public void AlterarExigencia_QuandoDesmarca_DeveDesligarAFlagERedirecionarParaIndex()
    {
        // Arrange (serviço 3: link e flag ligada)
        var modelo = CriarModeloDeAlteracaoDeExigencia(idServico: 3, exigeAnamnese: false);
        List<Servicoprofissional>? servicosEnviados = null;

        profissionalServiceMock
            .Setup(service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()))
            .Callback<uint, IEnumerable<Servicoprofissional>>((_, servicos) =>
                servicosEnviados = servicos.ToList());

        // Act
        var resultado = controller.AlterarExigencia(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, idProfissional);

        Assert.IsNotNull(servicosEnviados);
        Assert.AreEqual<sbyte>(0, servicosEnviados.Single().ExigeAnamnese);
    }

    [TestMethod]
    public void AlterarExigencia_QuandoServicoNaoExiste_NaoDeveGravarERedirecionarParaIndex()
    {
        // Arrange
        var modelo = CriarModeloDeAlteracaoDeExigencia(idServico: 999, exigeAnamnese: true);

        // Act
        var resultado = controller.AlterarExigencia(modelo);

        // Assert
        VerificarRedirecionamentoParaIndex(resultado, idProfissional);

        profissionalServiceMock.Verify(
            service => service.EditServicoProfissional(
                It.IsAny<uint>(),
                It.IsAny<IEnumerable<Servicoprofissional>>()),
            Times.Never);
    }

    private static void VerificarRedirecionamentoParaIndex(
        ActionResult resultado, uint idProfissionalEsperado)
    {
        var redirectResult =
            Assert.IsInstanceOfType<RedirectToActionResult>(resultado);

        Assert.IsNull(redirectResult.ControllerName);
        Assert.AreEqual(
            nameof(AnamneseController.Index),
            redirectResult.ActionName);
        Assert.AreEqual(
            idProfissionalEsperado,
            redirectResult.RouteValues!["idProfissional"]);
    }

    private static AnamneseViewModel CriarModeloDeNovaAnamnese()
    {
        return new AnamneseViewModel
        {
            IdProfissional = idProfissional,
            IdServico = 4,
            ExigeAnamnese = true,
            FormAnamnese = "https://forms.gle/novo"
        };
    }

    private static AnamneseViewModel CriarModeloDeAnamneseExistente()
    {
        return new AnamneseViewModel
        {
            IdProfissional = idProfissional,
            IdServico = 3,
            ExigeAnamnese = true,
            FormAnamnese = "https://forms.gle/abc123"
        };
    }

    private static AnamneseViewModel CriarModeloDeAlteracaoDeExigencia(uint idServico, bool exigeAnamnese)
    {
        return new AnamneseViewModel
        {
            IdProfissional = idProfissional,
            IdServico = idServico,
            ExigeAnamnese = exigeAnamnese
            // Sem FormAnamnese: o formulário do checkbox não envia o link
        };
    }

    private static Servicoprofissional ObterServicoAlvo()
    {
        return new Servicoprofissional
        {
            IdProfissional = idProfissional,
            IdEstabelecimento = idEstabelecimentoEsperado,
            IdServico = 3,
            ExigeAnamnese = 1,
            FormAnamnese = "https://forms.gle/abc123"
        };
    }

    private static Servicoprofissional ObterServicoSemAnamnese()
    {
        return new Servicoprofissional
        {
            IdProfissional = idProfissional,
            IdEstabelecimento = idEstabelecimentoEsperado,
            IdServico = 4,
            ExigeAnamnese = 0,
            FormAnamnese = string.Empty
        };
    }

    private static Servicoprofissional ObterServicoComLinkDesabilitado()
    {
        return new Servicoprofissional
        {
            IdProfissional = idProfissional,
            IdEstabelecimento = idEstabelecimentoEsperado,
            IdServico = 5,
            ExigeAnamnese = 0,
            FormAnamnese = "https://forms.gle/guardado"
        };
    }

    private static IEnumerable<Servicoprofissional> ObterServicosDeTeste()
    {
        return
        [
            new Servicoprofissional
            {
                IdProfissional = idProfissional,
                IdEstabelecimento = idEstabelecimentoEsperado,
                IdServico = 3,
                ExigeAnamnese = 1,
                FormAnamnese = "https://forms.gle/abc123"
            },
            new Servicoprofissional
            {
                IdProfissional = idProfissional,
                IdEstabelecimento = idEstabelecimentoEsperado,
                IdServico = 4,
                ExigeAnamnese = 0,
                FormAnamnese = string.Empty
            }
        ];
    }
}