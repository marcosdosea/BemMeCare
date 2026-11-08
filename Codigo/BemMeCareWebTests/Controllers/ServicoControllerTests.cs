using BemMeCareWeb.Controllers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BemMeCareWebTests.Controllers;

[TestClass]
public class ServicoControllerTests
{
    private Mock<IServicoService> mockService = null!;
    private ServicoController controller = null!;

    [TestInitialize]
    public void Initialize()
    {
        mockService = new Mock<IServicoService>();

        mockService.Setup(service => service.GetAll())
            .Returns(GetTestServicos());

        mockService.Setup(service => service.Get(1))
            .Returns(GetTargetServico());

        controller = new ServicoController(mockService.Object);
    }

    [TestMethod]
    public void IndexTest_Valido()
    {
        var result = controller.Index();

        Assert.IsInstanceOfType(result, typeof(ViewResult));

        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(
            viewResult.ViewData.Model,
            typeof(IEnumerable<ServicoViewModel>));

        var lista = ((IEnumerable<ServicoViewModel>)viewResult.ViewData.Model!).ToList();
        Assert.AreEqual(3, lista.Count);
    }

    [TestMethod]
    public void DetailsTest_Valido()
    {
        var result = controller.Details(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));

        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(ServicoViewModel));

        var servicoModel = (ServicoViewModel)viewResult.ViewData.Model!;
        Assert.AreEqual(1U, servicoModel.Id);
        Assert.AreEqual("Corte", servicoModel.Nome);
        Assert.AreEqual(1U, servicoModel.IdCategoria);
        Assert.AreEqual(1U, servicoModel.IdEstabelecimento);
    }

    [TestMethod]
    public void CreateTest_Get_Valido()
    {
        var result = controller.Create();

        Assert.IsInstanceOfType(result, typeof(ViewResult));

        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(ServicoViewModel));
    }

    [TestMethod]
    public void CreateTest_Post_Valido()
    {
        var result = controller.Create(GetNewServicoViewModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        var redirect = (RedirectToActionResult)result;
        Assert.IsNull(redirect.ControllerName);
        Assert.AreEqual("Index", redirect.ActionName);

        mockService.Verify(service => service.Create(
            It.Is<Servico>(s =>
                s.Nome == "Corte" &&
                s.IdCategoria == 1 &&
                s.IdEstabelecimento == 1)), Times.Once);
    }

    [TestMethod]
    public void CreateTest_Post_Invalido()
    {
        controller.ModelState.AddModelError("Nome", "Nome do serviço é obrigatório");

        var result = controller.Create(GetNewServicoViewModel());

        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        var redirect = (RedirectToActionResult)result;
        Assert.AreEqual("Index", redirect.ActionName);

        mockService.Verify(service => service.Create(It.IsAny<Servico>()), Times.Never);
    }

    [TestMethod]
    public void EditTest_Get_Valido()
    {
        var result = controller.Edit(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));

        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(ServicoViewModel));

        var servicoModel = (ServicoViewModel)viewResult.ViewData.Model!;
        Assert.AreEqual(1U, servicoModel.Id);
        Assert.AreEqual("Corte", servicoModel.Nome);
    }

    [TestMethod]
    public void EditTest_Post_Valido()
    {
        var result = controller.Edit(GetTargetServicoViewModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        var redirect = (RedirectToActionResult)result;
        Assert.IsNull(redirect.ControllerName);
        Assert.AreEqual("Index", redirect.ActionName);

        mockService.Verify(service => service.Edit(
            It.Is<Servico>(s =>
                s.Id == 1 &&
                s.Nome == "Corte" &&
                s.IdCategoria == 1 &&
                s.IdEstabelecimento == 1)), Times.Once);
    }

    [TestMethod]
    public void EditTest_Post_Invalido()
    {
        controller.ModelState.AddModelError("Nome", "Nome do serviço é obrigatório");

        var result = controller.Edit(GetTargetServicoViewModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        var redirect = (RedirectToActionResult)result;
        Assert.AreEqual("Index", redirect.ActionName);

        mockService.Verify(service => service.Edit(It.IsAny<Servico>()), Times.Never);
    }

    [TestMethod]
    public void DeleteTest_Get_Valido()
    {
        var result = controller.Delete(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));

        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(ServicoViewModel));

        var servicoModel = (ServicoViewModel)viewResult.ViewData.Model!;
        Assert.AreEqual(1U, servicoModel.Id);
        Assert.AreEqual("Corte", servicoModel.Nome);
    }

    [TestMethod]
    public void DeleteTest_Post_Valido()
    {
        var result = controller.Delete(GetTargetServicoViewModel());

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        var redirect = (RedirectToActionResult)result;
        Assert.IsNull(redirect.ControllerName);
        Assert.AreEqual("Index", redirect.ActionName);

        mockService.Verify(service => service.Delete(1), Times.Once);
    }

    private static ServicoViewModel GetNewServicoViewModel()
    {
        return new ServicoViewModel
        {
            Id = 1,
            Nome = "Corte",
            IdCategoria = 1,
            IdEstabelecimento = 1
        };
    }

    private static ServicoViewModel GetTargetServicoViewModel()
    {
        return GetNewServicoViewModel();
    }

    private static Servico GetTargetServico()
    {
        return new Servico
        {
            Id = 1,
            Nome = "Corte",
            IdCategoria = 1,
            IdEstabelecimento = 1
        };
    }

    private static IEnumerable<Servico> GetTestServicos()
    {
        return new List<Servico>
        {
            GetTargetServico(),
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
        };
    }
}