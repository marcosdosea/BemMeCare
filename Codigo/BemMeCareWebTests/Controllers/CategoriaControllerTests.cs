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
    private static CategoriaController controller;

    [TestInitialize]
    public void Initialize()
    {
        // Arrange
        var mockService = new Mock<ICategoriaService>();

        mockService.Setup(service => service.GetAll())
            .Returns(GetTestCategorias());
        mockService.Setup(service => service.Get(1))
            .Returns(GetTargetCategoria());
        mockService.Setup(service => service.Edit(It.IsAny<Categorium>()))
            .Verifiable();
        mockService.Setup(service => service.Create(It.IsAny<Categorium>()))
            .Returns(4);
        mockService.Setup(service => service.Delete(It.IsAny<uint>()))
            .Verifiable();
        controller = new CategoriaController(mockService.Object);
    }

    [TestMethod]
    public void IndexTest_Valido()
    {
        // Act
        var result = controller.Index();
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult) result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(List<CategoriaViewModel>));
        
        List<CategoriaViewModel> listaCategorias = (List<CategoriaViewModel>) viewResult.ViewData.Model;
        Assert.AreEqual(3, listaCategorias.Count);
    }

    [TestMethod]
    public void DetailsTest_Valido()
    {
        // Act
        var result = controller.Details(1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult) result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(CategoriaViewModel));
        CategoriaViewModel categoriaModel =  (CategoriaViewModel) viewResult.ViewData.Model;
        Assert.AreEqual(1U, categoriaModel.Id);
        Assert.AreEqual("Cabeleireiro", categoriaModel.Nome);
        Assert.AreEqual(1U, categoriaModel.IdEstabelecimento);
    }

    [TestMethod]
    public void CreateTest_Get_Valido()
    {
        // Act
        var result = controller.Create();
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }

    [TestMethod]
    public void CreateTest_Post_Valido()
    {
        // Act
        var result = controller.Create(GetNewCategoria());
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult) result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void CreateTest_Post_Invalido()
    {
        // Arrange
        controller.ModelState.AddModelError("Nome", "Nome da categoria é obrigatório");
        
        // Act
        var result = controller.Create(GetNewCategoria());
        
        // Assert
        Assert.AreEqual(1, controller.ModelState.ErrorCount);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult) result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void EditTest_Get_Valido()
    {
        // Act
        var result = controller.Edit(1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult) result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(CategoriaViewModel));
        CategoriaViewModel categoriaModel = (CategoriaViewModel) viewResult.ViewData.Model;
        Assert.AreEqual(1U, categoriaModel.Id);
        Assert.AreEqual("Cabeleireiro", categoriaModel.Nome);
        Assert.AreEqual(1U, categoriaModel.IdEstabelecimento);
    }

    [TestMethod]
    public void EditTest_Post_Valido()
    {
        // Act
        var result = controller.Edit(GetTargetCategoriaModel());
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult) result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    [TestMethod]
    public void DeleteTest_Get_Valido()
    {
        // Act
        var result = controller.Delete(1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        ViewResult viewResult = (ViewResult) result;
        Assert.IsInstanceOfType(viewResult.ViewData.Model, typeof(CategoriaViewModel));
        CategoriaViewModel categoriaModel = (CategoriaViewModel) viewResult.ViewData.Model;
        Assert.AreEqual(1U, categoriaModel.Id);
        Assert.AreEqual("Cabeleireiro", categoriaModel.Nome);
        Assert.AreEqual(1U, categoriaModel.IdEstabelecimento);
    }

    [TestMethod]
    public void DeleteTest_Post_Valido()
    {
        // Act
        var result = controller.Delete(GetTargetCategoriaModel());
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        RedirectToActionResult redirectToActionResult = (RedirectToActionResult) result;
        Assert.IsNull(redirectToActionResult.ControllerName);
        Assert.AreEqual("Index", redirectToActionResult.ActionName);
    }

    private CategoriaViewModel GetNewCategoria()
    {
        return new CategoriaViewModel
        {
            Id = 1,
            Nome = "Cabeleireiro",
            IdEstabelecimento = 1
        };
    }
    
    private Categorium GetTargetCategoria()
    {
        return new Categorium
        {
            Id = 1,
            Nome = "Cabeleireiro",
            IdEstabelecimento = 1
        };
    }

    private CategoriaViewModel GetTargetCategoriaModel()
    {
        return new CategoriaViewModel
        {
            Id = 1,
            Nome = "Cabeleireiro",
            IdEstabelecimento = 1
        };
    }
    
    private IEnumerable<Categorium> GetTestCategorias()
    {
        return new List<Categorium>
        {
            new Categorium
            {
                Id = 1,
                Nome = "Cabeleireiro",
                IdEstabelecimento = 1
            },
            new Categorium
            {
                Id = 2,
                Nome = "Cabeleireiro",
                IdEstabelecimento = 2
            },
            new Categorium
            {
                Id = 3,
                Nome = "Manicure",
                IdEstabelecimento = 1
            },
        };
    }
}