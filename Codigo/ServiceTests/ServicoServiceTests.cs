using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class ServicoServiceTests
{
    private BemMeCareContext contexto = null!;
    private IServicoService servico = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase($"BemMeCare_{Guid.NewGuid()}")
            .Options;

        contexto = new BemMeCareContext(opcoes);
        contexto.Servicos.AddRange(ObterServicosDeTeste());
        contexto.SaveChanges();

        servico = new ServicoService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoServicoCriado()
    {
        var novoServico = CriarNovoServico();

        var idRetornado = servico.Create(novoServico);

        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarServico()
    {
        var servicoEsperado = CriarNovoServico();

        servico.Create(servicoEsperado);

        var servicoCriado = servico.Get(4);

        Assert.IsNotNull(servicoCriado);
        VerificarDados(servicoEsperado, servicoCriado);
    }

    [TestMethod]
    public void Delete_QuandoServicoExiste_DeveRemoverServico()
    {
        servico.Delete(2);

        var servicoExcluido = servico.Get(2);
        var servicosRestantes = servico.GetAll().ToList();

        Assert.IsNull(servicoExcluido);
        Assert.HasCount(2, servicosRestantes);
        Assert.IsFalse(servicosRestantes.Any(item => item.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoServicoNaoExiste_NaoDeveAlterarOsDados()
    {
        var idsIniciais = servico.GetAll()
            .Select(item => item.Id)
            .ToList();

        servico.Delete(999);

        var idsFinais = servico.GetAll()
            .Select(item => item.Id)
            .ToList();

        CollectionAssert.AreEquivalent(idsIniciais, idsFinais);
    }

    [TestMethod]
    public void Edit_QuandoServicoExiste_DeveAtualizarSeusDados()
    {
        var servicoEditado = servico.Get(3);

        Assert.IsNotNull(servicoEditado);

        servicoEditado!.Nome = "Barba atualizada";
        servicoEditado.IdCategoria = 2;
        servicoEditado.IdEstabelecimento = 1;

        servico.Edit(servicoEditado);

        var resultado = servico.Get(3);

        Assert.IsNotNull(resultado);
        Assert.AreEqual("Barba atualizada", resultado.Nome);
        Assert.AreEqual(2u, resultado.IdCategoria);
        Assert.AreEqual(1u, resultado.IdEstabelecimento);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarServicoCorrespondente()
    {
        var resultado = servico.Get(1);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(1u, resultado.Id);
        Assert.AreEqual("Corte", resultado.Nome);
        Assert.AreEqual(1u, resultado.IdCategoria);
        Assert.AreEqual(1u, resultado.IdEstabelecimento);
    }

    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        var resultado = servico.Get(999);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void GetAll_QuandoExistemServicos_DeveRetornarTodosOsServicos()
    {
        var resultados = servico.GetAll().ToList();

        Assert.HasCount(3, resultados);
        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2, 3 },
            resultados.Select(item => item.Id).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaServicos_DeveRetornarColecaoVazia()
    {
        contexto.Servicos.RemoveRange(contexto.Servicos);
        contexto.SaveChanges();

        var resultados = servico.GetAll().ToList();

        Assert.IsEmpty(resultados);
    }

    [TestMethod]
    public void GetByNome_QuandoExisteCorrespondencia_DeveRetornarServicosCorrespondentes()
    {
        var resultados = servico.GetByNome("Cor").ToList();

        Assert.HasCount(1, resultados);
        Assert.AreEqual("Corte", resultados[0].Nome);
    }

    [TestMethod]
    public void GetByNome_QuandoNaoExisteCorrespondencia_DeveRetornarColecaoVazia()
    {
        var resultados = servico.GetByNome("Inexistente").ToList();

        Assert.IsEmpty(resultados);
    }

    [TestMethod]
    public void GetByNome_QuandoNomeEstaVazio_DeveRetornarTodosOsServicos()
    {
        var resultados = servico.GetByNome(string.Empty).ToList();

        Assert.HasCount(3, resultados);
    }

    private static Servico CriarNovoServico()
    {
        return new Servico
        {
            Id = 4,
            Nome = "Massagem",
            IdCategoria = 2,
            IdEstabelecimento = 1
        };
    }

    private static IEnumerable<Servico> ObterServicosDeTeste()
    {
        return
        [
            new Servico
            {
                Id = 1,
                Nome = "Corte",
                IdCategoria = 1,
                IdEstabelecimento = 1
            },
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
        ];
    }

    private static void VerificarDados(Servico esperado, Servico atual)
    {
        Assert.AreEqual(esperado.Id, atual.Id);
        Assert.AreEqual(esperado.Nome, atual.Nome);
        Assert.AreEqual(esperado.IdCategoria, atual.IdCategoria);
        Assert.AreEqual(esperado.IdEstabelecimento, atual.IdEstabelecimento);
    }
}