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

        contexto.Servicos.AddRange(
            new Servico { Id = 1, Nome = "Escova", IdCategoria = 1, IdEstabelecimento = 1 },
            new Servico { Id = 2, Nome = "Botox", IdCategoria = 1, IdEstabelecimento = 1 },
            new Servico { Id = 3, Nome = "Corte", IdCategoria = 1, IdEstabelecimento = 2 });

        // O profissional 1 atua nos estabelecimentos 1 e 2
        contexto.Servicoprofissionals.AddRange(
            new Servicoprofissional
            {
                IdProfissional = 1,
                IdEstabelecimento = 1,
                IdServico = 1,
                ExigeAnamnese = 1,
                FormAnamnese = "https://forms.gle/abc123",
                Preco = 35m,
                PercentualSinal = 40f,
                QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO",
                Duracao = 45
            },
            new Servicoprofissional
            {
                IdProfissional = 1,
                IdEstabelecimento = 1,
                IdServico = 2,
                ExigeAnamnese = 0,
                FormAnamnese = string.Empty,
                Preco = 70m,
                PercentualSinal = 50f,
                QuandoPagarSinal = "TODO_AGENDAMENTO",
                Duracao = 60
            },
            new Servicoprofissional
            {
                IdProfissional = 1,
                IdEstabelecimento = 2,
                IdServico = 3,
                ExigeAnamnese = 0,
                FormAnamnese = string.Empty,
                Preco = 50m,
                PercentualSinal = 0f,
                QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO",
                Duracao = 30
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
        Assert.AreEqual(profissionalEsperado.GerirHorario, profissionalCriado.GerirHorario);
        Assert.AreEqual(profissionalEsperado.EstaAtivo, profissionalCriado.EstaAtivo);
        Assert.AreEqual(profissionalEsperado.GerirServico, profissionalCriado.GerirServico);
        Assert.AreEqual(profissionalEsperado.ReceberPagamentoDireto, profissionalCriado.ReceberPagamentoDireto);
        Assert.AreEqual(profissionalEsperado.ChavePix, profissionalCriado.ChavePix);
        Assert.AreEqual(profissionalEsperado.DuracaoIntervaloTempo, profissionalCriado.DuracaoIntervaloTempo);
        Assert.AreEqual(profissionalEsperado.EProfissional, profissionalCriado.EProfissional);
        Assert.AreEqual(profissionalEsperado.EAtendente, profissionalCriado.EAtendente);
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

        profissional!.GerirHorario = 0;
        profissional.EstaAtivo = 1;
        profissional.GerirServico = 0;
        profissional.ReceberPagamentoDireto = 0;
        profissional.ChavePix = "novachave@pix.com";
        profissional.DuracaoIntervaloTempo = 45;
        profissional.EProfissional = 0;
        profissional.EAtendente = 0;

        // Act
        servicoProfissional.Edit(profissional);

        // Assert
        var profissionalAtualizado = servicoProfissional.Get(3);

        Assert.IsNotNull(profissionalAtualizado);
        Assert.AreEqual(3u, profissionalAtualizado.IdPessoa);
        Assert.AreEqual(1u, profissionalAtualizado.IdEstabelecimento);
        Assert.AreEqual((sbyte)0, profissionalAtualizado.GerirHorario);
        Assert.AreEqual((sbyte)1, profissionalAtualizado.EstaAtivo);
        Assert.AreEqual((sbyte)0, profissionalAtualizado.GerirServico);
        Assert.AreEqual((sbyte)0, profissionalAtualizado.ReceberPagamentoDireto);
        Assert.AreEqual("novachave@pix.com", profissionalAtualizado.ChavePix);
        Assert.AreEqual(45u, profissionalAtualizado.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)0, profissionalAtualizado.EProfissional);
        Assert.AreEqual((sbyte?)0, profissionalAtualizado.EAtendente);
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
        Assert.AreEqual((sbyte)1, profissional.GerirHorario);
        Assert.AreEqual((sbyte)1, profissional.EstaAtivo);
        Assert.AreEqual((sbyte)1, profissional.GerirServico);
        Assert.AreEqual((sbyte)0, profissional.ReceberPagamentoDireto);
        Assert.AreEqual("prof1@pix.com", profissional.ChavePix);
        Assert.AreEqual(15u, profissional.DuracaoIntervaloTempo);
        Assert.AreEqual((sbyte?)1, profissional.EProfissional);
        Assert.AreEqual((sbyte?)0, profissional.EAtendente);
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

    [TestMethod]
    public void GetServicoProfissional_QuandoProfissionalAtuaEmDoisEstabelecimentos_DeveRetornarSoOsDoEstabelecimentoInformado()
    {
        // Act
        var servicos = servicoProfissional.GetServicoProfissional(1, 1).ToList();

        // Assert
        Assert.AreEqual(2, servicos.Count);
        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2 },
            servicos.Select(s => s.IdServico).ToList());
        Assert.IsTrue(servicos.All(s => s.IdEstabelecimento == 1));
        Assert.AreEqual("Escova", servicos.Single(s => s.IdServico == 1).IdServicoNavigation.Nome);
    }

    [TestMethod]
    public void GetServicoProfissional_QuandoProfissionalNaoPossuiServicos_DeveRetornarColecaoVazia()
    {
        // Act
        var servicos = servicoProfissional.GetServicoProfissional(2, 1).ToList();

        // Assert
        Assert.AreEqual(0, servicos.Count);
    }

    [TestMethod]
    public void GetServicoProfissional_QuandoServicoExiste_DeveRetornarOServicoComAnamnese()
    {
        // Act
        var servico = servicoProfissional.GetServicoProfissional(1, 1, 1);

        // Assert
        Assert.IsNotNull(servico);
        Assert.AreEqual(1u, servico.IdProfissional);
        Assert.AreEqual(1u, servico.IdServico);
        Assert.AreEqual(1u, servico.IdEstabelecimento);
        Assert.AreEqual((sbyte)1, servico.ExigeAnamnese);
        Assert.AreEqual("https://forms.gle/abc123", servico.FormAnamnese);
    }

    [TestMethod]
    public void GetServicoProfissional_QuandoEstabelecimentoNaoCorresponde_DeveRetornarNulo()
    {
        // Act (o serviço 1 existe, mas só no estabelecimento 1)
        var servico = servicoProfissional.GetServicoProfissional(1, 1, 2);

        // Assert
        Assert.IsNull(servico);
    }

    [TestMethod]
    public void EditServicoProfissional_QuandoMarcaComLink_DeveAlterarSoOsCamposDaAnamnese()
    {
        // Arrange (o serviço 2 custa 70 e dura 60; o mapper entrega Preco e Duracao zerados)
        var alteracao = new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 1,
            IdServico = 2,
            ExigeAnamnese = 1,
            FormAnamnese = "https://forms.gle/novo"
        };

        // Act
        servicoProfissional.EditServicoProfissional(1, [alteracao]);

        // Assert
        var salvo = ObterServicoSalvo(idServico: 2);

        Assert.AreEqual((sbyte)1, salvo.ExigeAnamnese);
        Assert.AreEqual("https://forms.gle/novo", salvo.FormAnamnese);
        Assert.AreEqual(70m, salvo.Preco);
        Assert.AreEqual(60u, salvo.Duracao);
        Assert.AreEqual(50f, salvo.PercentualSinal);
        Assert.AreEqual("TODO_AGENDAMENTO", salvo.QuandoPagarSinal);
    }

    [TestMethod]
    public void EditServicoProfissional_QuandoExigeAnamneseSemLink_NaoDeveAlterarOServico()
    {
        // Arrange (a regra: exigir anamnese só vale com o link preenchido)
        var alteracao = new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 1,
            IdServico = 2,
            ExigeAnamnese = 1,
            FormAnamnese = "  "
        };

        // Act
        servicoProfissional.EditServicoProfissional(1, [alteracao]);

        // Assert
        var salvo = ObterServicoSalvo(idServico: 2);

        Assert.AreEqual((sbyte)0, salvo.ExigeAnamnese);
        Assert.AreEqual(string.Empty, salvo.FormAnamnese);
    }

    [TestMethod]
    public void EditServicoProfissional_QuandoRemoveAAnamnese_DeveLimparOLinkEDesligarAFlag()
    {
        // Arrange (é o que o Delete do controller envia)
        var alteracao = new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 1,
            IdServico = 1,
            ExigeAnamnese = 0,
            FormAnamnese = string.Empty
        };

        // Act
        servicoProfissional.EditServicoProfissional(1, [alteracao]);

        // Assert
        var salvo = ObterServicoSalvo(idServico: 1);

        Assert.AreEqual((sbyte)0, salvo.ExigeAnamnese);
        Assert.AreEqual(string.Empty, salvo.FormAnamnese);
        Assert.AreEqual(35m, salvo.Preco);
        Assert.AreEqual(45u, salvo.Duracao);
    }

    [TestMethod]
    public void EditServicoProfissional_QuandoEstabelecimentoNaoCorresponde_NaoDeveAlterarNada()
    {
        // Arrange (o serviço 1 só existe no estabelecimento 1)
        var alteracao = new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 2,
            IdServico = 1,
            ExigeAnamnese = 1,
            FormAnamnese = "https://forms.gle/invasor"
        };

        // Act
        servicoProfissional.EditServicoProfissional(1, [alteracao]);

        // Assert
        var original = ObterServicoSalvo(idServico: 1);

        Assert.AreEqual("https://forms.gle/abc123", original.FormAnamnese);
        Assert.IsFalse(contexto.Servicoprofissionals.Any(sp =>
            sp.IdEstabelecimento == 2 && sp.IdServico == 1));
    }

    [TestMethod]
    public void EditServicoProfissional_QuandoServicoNaoExiste_NaoDeveCriarRegistro()
    {
        // Arrange
        var alteracao = new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 1,
            IdServico = 999,
            ExigeAnamnese = 1,
            FormAnamnese = "https://forms.gle/novo"
        };

        // Act
        servicoProfissional.EditServicoProfissional(1, [alteracao]);

        // Assert
        Assert.AreEqual(3, contexto.Servicoprofissionals.Count());
        Assert.IsFalse(contexto.Servicoprofissionals.Any(sp => sp.IdServico == 999));
    }

    private Servicoprofissional ObterServicoSalvo(uint idServico, uint idEstabelecimento = 1)
    {
        return contexto.Servicoprofissionals.Single(sp =>
            sp.IdProfissional == 1 &&
            sp.IdEstabelecimento == idEstabelecimento &&
            sp.IdServico == idServico);
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