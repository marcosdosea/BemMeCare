using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class CategoriaServiceTests
{
    private BemMeCareContext contexto = null!;
    private ICategoriaService servicoCategoria = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var nomeBanco = $"BemMeCare_{Guid.NewGuid()}";

        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        contexto = new BemMeCareContext(opcoes);

        contexto.Categoria.AddRange(
            new Categorium
            {
                Id = 1,
                Nome = "Cabeleireiro",
                IdEstabelecimento = 1
            },
            new Categorium
            {
                Id = 2,
                Nome = "Manicure",
                IdEstabelecimento = 1
            },
            new Categorium
            {
                Id = 3,
                Nome = "Esteticista",
                IdEstabelecimento = 2
            });

        contexto.SaveChanges();

        servicoCategoria = new CategoriaService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDaCategoriaCriada()
    {
        // Arrange
        var categoria = new Categorium
        {
            Id = 4,
            Nome = "Barbeiro",
            IdEstabelecimento = 2
        };

        // Act
        var idRetornado = servicoCategoria.Create(categoria);

        // Assert
        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarCategoria()
    {
        // Arrange
        var categoriaEsperada = new Categorium
        {
            Id = 4,
            Nome = "Barbeiro",
            IdEstabelecimento = 2
        };

        // Act
        servicoCategoria.Create(categoriaEsperada);

        // Assert
        var categoriaCriada = servicoCategoria.Get(4, 2);

        Assert.IsNotNull(categoriaCriada);
        Assert.AreEqual(categoriaEsperada.Id, categoriaCriada.Id);
        Assert.AreEqual(categoriaEsperada.Nome, categoriaCriada.Nome);
        Assert.AreEqual(categoriaEsperada.IdEstabelecimento, categoriaCriada.IdEstabelecimento);
    }

    [TestMethod]
    public void Delete_QuandoCategoriaExiste_DeveRemoverCategoria()
    {
        // Act
        servicoCategoria.Delete(2, 1);

        // Assert
        var categoriaExcluida = servicoCategoria.Get(2, 1);
        var categoriasRestantes = servicoCategoria.GetAll(1).ToList();

        Assert.IsNull(categoriaExcluida);
        Assert.HasCount(1, categoriasRestantes);
        Assert.IsFalse(categoriasRestantes.Any(categoria => categoria.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoCategoriaNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var categoriasIniciais = servicoCategoria.GetAll(1).ToList();

        // Act
        servicoCategoria.Delete(999, 1);

        // Assert
        var categoriasFinais = servicoCategoria.GetAll(1).ToList();

        Assert.HasCount(categoriasIniciais.Count, categoriasFinais);
        CollectionAssert.AreEquivalent(
            categoriasIniciais.Select(categoria => categoria.Id).ToList(),
            categoriasFinais.Select(categoria => categoria.Id).ToList());
    }

    [TestMethod]
    public void Edit_QuandoCategoriaExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var categoria = servicoCategoria.Get(3, 2);

        Assert.IsNotNull(categoria);

        categoria!.Nome = "Barbeiro";

        // Act
        servicoCategoria.Edit(categoria);

        // Assert
        var categoriaAtualizado = servicoCategoria.Get(3, 2);

        Assert.IsNotNull(categoriaAtualizado);
        Assert.AreEqual("Barbeiro", categoriaAtualizado.Nome);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarCategoriaCorrespondente()
    {
        // Act
        var categoria = servicoCategoria.Get(1, 1);

        // Assert
        Assert.IsNotNull(categoria);
        Assert.AreEqual(1u, categoria.Id);
        Assert.AreEqual("Cabeleireiro", categoria.Nome);
        Assert.AreEqual(1u, categoria.IdEstabelecimento);
    }


    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        // Act
        var categoria = servicoCategoria.Get(999, 1);

        // Assert
        Assert.IsNull(categoria);
    }

    [TestMethod]
    public void GetAll_QuandoExistemCategorias_DeveRetornarTodosAsCategorias()
    {
        // Act
        var categorias = servicoCategoria.GetAll(1).ToList();

        // Assert
        Assert.IsNotNull(categorias);
        Assert.HasCount(2, categorias);

        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2 },
            categorias.Select(categoria => categoria.Id).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaCategorias_DeveRetornarColecaoVazia()
    {
        // Arrange
        contexto.Categoria.RemoveRange(contexto.Categoria);
        contexto.SaveChanges();

        // Act
        var categorias = servicoCategoria.GetAll(1).ToList();

        // Assert
        Assert.IsNotNull(categorias);
        Assert.IsEmpty(categorias);
    }
}