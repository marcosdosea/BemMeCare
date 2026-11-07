using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service.Tests;

[TestClass]
public class ProfissionalServiceTests
{
    private BemMeCareContext contexto = null!;
    private IProfissionalService servicoProfissional = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var nomeBanco = $"BemMeCare_{Guid.NewGuid()}";

        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        contexto = new BemMeCareContext(opcoes);

        contexto.Profissionals.AddRange(
            new Profissional
            {
                IdPessoa = 1,
                IdEstabelecimento = 1,
                GerirHorario = 1,
                EstaAtivo = 1,
                GerirServico = 1,
                ReceberPagamentoDireto = 0,
                ChavePix = "prof1@pix.com",
                DuracaoIntervaloTempo = 15,
                EProfissional = 1,
                EAtendente = 0
            },
            new Profissional
            {
                IdPessoa = 2,
                IdEstabelecimento = 1,
                GerirHorario = 0,
                EstaAtivo = 1,
                GerirServico = 0,
                ReceberPagamentoDireto = 1,
                ChavePix = "prof2@pix.com",
                DuracaoIntervaloTempo = 30,
                EProfissional = 1,
                EAtendente = 0
            },
            new Profissional
            {
                IdPessoa = 3,
                IdEstabelecimento = 1,
                GerirHorario = 1,
                EstaAtivo = 0,
                GerirServico = 1,
                ReceberPagamentoDireto = 1,
                ChavePix = "prof3@pix.com",
                DuracaoIntervaloTempo = 20,
                EProfissional = 1,
                EAtendente = 1
            });

        contexto.SaveChanges();

        servicoProfissional = new ProfissionalService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoProfissionalCriado()
    {
        // Arrange
        var profissional = CriarNovoProfissional();

        // Act
        var idRetornado = servicoProfissional.Create(profissional);

        // Assert
        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarProfissional()
    {
        // Arrange
        var profissionalEsperado = CriarNovoProfissional();

        // Act
        servicoProfissional.Create(profissionalEsperado);

        // Assert
        var profissionalCriado = servicoProfissional.Get(profissionalEsperado.IdPessoa);

        Assert.IsNotNull(profissionalCriado);
        Assert.AreEqual(profissionalEsperado.IdPessoa, profissionalCriado.IdPessoa);
        Assert.AreEqual(profissionalEsperado.IdEstabelecimento, profissionalCriado.IdEstabelecimento);
        Assert.AreEqual(profissionalEsperado.ChavePix, profissionalCriado.ChavePix);
        Assert.AreEqual(profissionalEsperado.EstaAtivo, profissionalCriado.EstaAtivo);
        Assert.AreEqual(profissionalEsperado.GerirHorario, profissionalCriado.GerirHorario);
        Assert.AreEqual(profissionalEsperado.GerirServico, profissionalCriado.GerirServico);
        Assert.AreEqual(profissionalEsperado.ReceberPagamentoDireto, profissionalCriado.ReceberPagamentoDireto);
    }

    [TestMethod]
    public void Delete_QuandoProfissionalExiste_DeveRemoverProfissional()
    {
        // Act
        servicoProfissional.Delete(2);

        // Assert
        var profissionalExcluido = servicoProfissional.Get(2);
        var profissionaisRestantes = servicoProfissional.GetAll().ToList();

        Assert.IsNull(profissionalExcluido);
        Assert.AreEqual(2, profissionaisRestantes.Count);
        Assert.IsFalse(profissionaisRestantes.Any(p => p.IdPessoa == 2));
    }

    [TestMethod]
    public void Delete_QuandoProfissionalNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var profissionaisIniciais = servicoProfissional.GetAll().ToList();

        // Act
        servicoProfissional.Delete(999);

        // Assert
        var profissionaisFinais = servicoProfissional.GetAll().ToList();

        Assert.AreEqual(profissionaisIniciais.Count, profissionaisFinais.Count);
        CollectionAssert.AreEquivalent(
            profissionaisIniciais.Select(p => p.IdPessoa).ToList(),
            profissionaisFinais.Select(p => p.IdPessoa).ToList());
    }

    [TestMethod]
    public void Edit_QuandoProfissionalExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var profissional = servicoProfissional.Get(3);

        Assert.IsNotNull(profissional);

        profissional!.ChavePix = "novachave@pix.com";
        profissional.EstaAtivo = 1;
        profissional.DuracaoIntervaloTempo = 45;

        // Act
        servicoProfissional.Edit(profissional);

        // Assert
        var profissionalAtualizado = servicoProfissional.Get(3);

        Assert.IsNotNull(profissionalAtualizado);
        Assert.AreEqual("novachave@pix.com", profissionalAtualizado.ChavePix);
        Assert.AreEqual((sbyte)1, profissionalAtualizado.EstaAtivo);
        Assert.AreEqual(45u, profissionalAtualizado.DuracaoIntervaloTempo);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarProfissionalCorrespondente()
    {
        // Act
        var profissional = servicoProfissional.Get(1);

        // Assert
        Assert.IsNotNull(profissional);
        Assert.AreEqual(1u, profissional.IdPessoa);
        Assert.AreEqual(1u, profissional.IdEstabelecimento);
        Assert.AreEqual("prof1@pix.com", profissional.ChavePix);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
    }

    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        // Act
        var profissional = servicoProfissional.Get(999);

        // Assert
        Assert.IsNull(profissional);
    }

    [TestMethod]
    public void GetAll_QuandoExistemProfissionais_DeveRetornarTodosOsProfissionais()
    {
        // Act
        var profissionais = servicoProfissional.GetAll().ToList();

        // Assert
        Assert.IsNotNull(profissionais);
        Assert.AreEqual(3, profissionais.Count);

        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2, 3 },
            profissionais.Select(p => p.IdPessoa).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaProfissionais_DeveRetornarColecaoVazia()
    {
        // Arrange
        contexto.Profissionals.RemoveRange(contexto.Profissionals);
        contexto.SaveChanges();

        // Act
        var profissionais = servicoProfissional.GetAll().ToList();

        // Assert
        Assert.IsNotNull(profissionais);
        Assert.AreEqual(0, profissionais.Count);
    }

    private static Profissional CriarNovoProfissional()
    {
        return new Profissional
        {
            IdPessoa = 4,
            IdEstabelecimento = 1,
            GerirHorario = 1,
            EstaAtivo = 1,
            GerirServico = 0,
            ReceberPagamentoDireto = 1,
            ChavePix = "prof4@pix.com",
            DuracaoIntervaloTempo = 15,
            EProfissional = 1,
            EAtendente = 0
        };
    }
}
