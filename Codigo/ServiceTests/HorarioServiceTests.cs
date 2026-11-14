using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class HorarioServiceTests
{
    private BemMeCareContext contexto = null!;
    private IHorarioService servicoHorario = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var nomeBanco = $"BemMeCare_{Guid.NewGuid()}";

        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        contexto = new BemMeCareContext(opcoes);

        contexto.Horarios.AddRange(
            new Horario
            {
                Id = 1,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Segunda-feira",
                IdProfissional = 1,
                IdEstabelecimento = 1
            },
            new Horario
            {
                Id = 2,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Terça-feira",
                IdProfissional = 1,
                IdEstabelecimento = 1
            },
            new Horario
            {
                Id = 3,
                InicioTurno = new TimeSpan(9, 0, 0),
                FimTurno = new TimeSpan(17, 0, 0),
                InicioVigencia = new DateTime(2024, 1, 1),
                FimVigencia = null,
                DiaSemana = "Quarta-feira",
                IdProfissional = 2,
                IdEstabelecimento = 1
            });

        contexto.SaveChanges();

        servicoHorario = new HorarioService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoHorarioCriado()
    {
        // Arrange
        var horario = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Quinta-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act
        var idRetornado = servicoHorario.Create(horario);

        // Assert
        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarHorario()
    {
        // Arrange
        var horarioEsperado = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Quinta-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act
        servicoHorario.Create(horarioEsperado);

        // Assert
        var horarioCriado = servicoHorario.Get(horarioEsperado.Id, 1);

        Assert.IsNotNull(horarioCriado);
        Assert.AreEqual(horarioEsperado.Id, horarioCriado.Id);
        Assert.AreEqual(horarioEsperado.InicioTurno, horarioCriado.InicioTurno);
        Assert.AreEqual(horarioEsperado.FimTurno, horarioCriado.FimTurno);
        Assert.AreEqual(horarioEsperado.InicioVigencia, horarioCriado.InicioVigencia);
        Assert.AreEqual(horarioEsperado.FimVigencia, horarioCriado.FimVigencia);
        Assert.AreEqual(horarioEsperado.DiaSemana, horarioCriado.DiaSemana);
        Assert.AreEqual(horarioEsperado.IdProfissional, horarioCriado.IdProfissional);
        Assert.AreEqual(horarioEsperado.IdEstabelecimento, horarioCriado.IdEstabelecimento);
    }

    [TestMethod]
    public void Delete_QuandoHorarioExiste_DeveRemoverHorario()
    {
        // Act
        servicoHorario.Delete(2, 1);

        // Assert
        var horarioExcluido = servicoHorario.Get(2, 1);
        var horariosRestantes = servicoHorario.GetAll(1).ToList();

        Assert.IsNull(horarioExcluido);
        Assert.HasCount(2, horariosRestantes);
        Assert.IsFalse(horariosRestantes.Any(horario => horario.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoHorarioNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var horariosIniciais = servicoHorario.GetAll(1).ToList();

        // Act
        servicoHorario.Delete(999, 1);

        // Assert
        var horariosFinais = servicoHorario.GetAll(1).ToList();

        Assert.HasCount(horariosIniciais.Count, horariosFinais);
        CollectionAssert.AreEquivalent(
            horariosIniciais.Select(horario => horario.Id).ToList(),
            horariosFinais.Select(horario => horario.Id).ToList());
    }

    [TestMethod]
    public void Edit_QuandoHorarioExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var horario = servicoHorario.Get(1, 1);

        Assert.IsNotNull(horario);

        horario.InicioTurno = new TimeSpan(10, 0, 0);
        horario.FimTurno = new TimeSpan(18, 0, 0);
        horario.InicioVigencia = new DateTime(2023, 1, 1);
        horario.FimVigencia = new DateTime(2023, 12, 31);
        horario.DiaSemana = "Sábado";
        horario.IdProfissional = 1;
        horario.IdEstabelecimento = 1;

        // Act
        servicoHorario.Edit(horario, 1);

        // Assert
        var horarioAtualizado = servicoHorario.Get(1, 1);

        Assert.IsNotNull(horarioAtualizado);
        Assert.AreEqual(1u, horarioAtualizado.Id);
        Assert.AreEqual(new TimeSpan(10, 0, 0), horarioAtualizado.InicioTurno);
        Assert.AreEqual(new TimeSpan(18, 0, 0), horarioAtualizado.FimTurno);
        Assert.AreEqual(new DateTime(2023, 1, 1), horarioAtualizado.InicioVigencia);
        Assert.AreEqual(new DateTime(2023, 12, 31), horarioAtualizado.FimVigencia);
        Assert.AreEqual("Sábado", horarioAtualizado.DiaSemana);
        Assert.AreEqual(1u, horarioAtualizado.IdProfissional);
        Assert.AreEqual(1u, horarioAtualizado.IdEstabelecimento);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarHorarioCorrespondente()
    {
        // Act
        var horario = servicoHorario.Get(1, 1);

        // Assert
        Assert.IsNotNull(horario);
        Assert.AreEqual(1u, horario.Id);
        Assert.AreEqual(new TimeSpan(9, 0, 0), horario.InicioTurno);
        Assert.AreEqual(new TimeSpan(17, 0, 0), horario.FimTurno);
        Assert.AreEqual(new DateTime(2024, 1, 1), horario.InicioVigencia);
        Assert.IsNull(horario.FimVigencia);
        Assert.AreEqual("Segunda-feira", horario.DiaSemana);
        Assert.AreEqual(1u, horario.IdProfissional);
        Assert.AreEqual(1u, horario.IdEstabelecimento);
    }

    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        // Act
        var horario = servicoHorario.Get(999, 1);

        // Assert
        Assert.IsNull(horario);
    }

    [TestMethod]
    public void GetAll_QuandoExistemHorarios_DeveRetornarTodosOsHorarios()
    {
        // Act
        var horarios = servicoHorario.GetAll(1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.HasCount(3, horarios);

        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2, 3 },
            horarios.Select(horario => horario.Id).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaHorarios_DeveRetornarColecaoVazia()
    {
        // Arrange
        contexto.Horarios.RemoveRange(contexto.Horarios);
        contexto.SaveChanges();

        // Act
        var horarios = servicoHorario.GetAll(1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByProfissional_QuandoExistemHorariosParaOProfissional_DeveRetornarHorariosCorrespondentes()
    {
        // Act
        var horarios = servicoHorario.GetByProfissional(1, 1).ToList();

        // Assert
        Assert.HasCount(2, horarios);
        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2 },
            horarios.Select(h => h.Id).ToList());
        Assert.IsTrue(horarios.All(h => h.IdProfissional == 1 && h.IdEstabelecimento == 1));
    }

    [TestMethod]
    public void GetByProfissional_QuandoNaoExistemHorariosParaOProfissional_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByProfissional(999, 1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoExisteCorrespondencia_DeveRetornarHorariosCorrespondentes()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Segunda-feira", 1, 1).ToList();

        // Assert
        Assert.HasCount(1, horarios);
        Assert.AreEqual(1u, horarios[0].Id);
        Assert.AreEqual("Segunda-feira", horarios[0].DiaSemana);
        Assert.AreEqual(1u, horarios[0].IdProfissional);
        Assert.AreEqual(1u, horarios[0].IdEstabelecimento);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoDiaExisteMasProfissionalDiferente_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Quarta-feira", 1, 1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoProfissionalExisteMasDiaDiferente_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Domingo", 1, 1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    // =========================================================================
    // NOVOS TESTES IMPLEMENTADOS: Restrições de Estabelecimento e Sobreposição
    // =========================================================================

    [TestMethod]
    public void Delete_QuandoHorarioPertenceAOutroEstabelecimento_NaoDeveRemover()
    {
        // Act (Horário 1 pertence ao estabelecimento 1, tentando remover pelo estabelecimento 2)
        servicoHorario.Delete(1, 2);

        // Assert
        var horario = servicoHorario.Get(1, 1);
        Assert.IsNotNull(horario);
    }

    [TestMethod]
    public void Edit_QuandoHorarioPertenceAOutroEstabelecimento_NaoDeveAtualizar()
    {
        // Arrange
        var alteracao = new Horario
        {
            Id = 1,
            InicioTurno = new TimeSpan(12, 0, 0),
            FimTurno = new TimeSpan(20, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act (Tentando editar o horário 1 fornecendo o estabelecimento 2)
        servicoHorario.Edit(alteracao, 2);

        // Assert (Os dados originais do horário 1 devem permanecer inalterados)
        var horarioOriginal = servicoHorario.Get(1, 1);
        Assert.IsNotNull(horarioOriginal);
        Assert.AreEqual(new TimeSpan(9, 0, 0), horarioOriginal.InicioTurno);
        Assert.AreEqual(new TimeSpan(17, 0, 0), horarioOriginal.FimTurno);
    }

    [TestMethod]
    public void Get_QuandoHorarioPertenceAOutroEstabelecimento_DeveRetornarNulo()
    {
        // Act (Horário 1 pertence ao estabelecimento 1)
        var horario = servicoHorario.Get(1, 2);

        // Assert
        Assert.IsNull(horario);
    }

    [TestMethod]
    public void GetAll_QuandoFiltradoPorEstabelecimento_DeveRetornarApenasDoEstabelecimento()
    {
        // Arrange
        contexto.Horarios.Add(new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Sexta-feira",
            IdProfissional = 1,
            IdEstabelecimento = 2
        });
        contexto.SaveChanges();

        // Act
        var horariosEstabelecimento1 = servicoHorario.GetAll(1).ToList();
        var horariosEstabelecimento2 = servicoHorario.GetAll(2).ToList();

        // Assert
        Assert.HasCount(3, horariosEstabelecimento1);
        Assert.IsTrue(horariosEstabelecimento1.All(h => h.IdEstabelecimento == 1));

        Assert.HasCount(1, horariosEstabelecimento2);
        Assert.AreEqual(4u, horariosEstabelecimento2[0].Id);
        Assert.AreEqual(2u, horariosEstabelecimento2[0].IdEstabelecimento);
    }

    [TestMethod]
    public void GetByProfissional_QuandoProfissionalTemHorarioEmOutroEstabelecimento_DeveRetornarApenasDoEstabelecimentoInformado()
    {
        // Arrange (Profissional 1 recebe horário no estabelecimento 2)
        contexto.Horarios.Add(new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Sexta-feira",
            IdProfissional = 1,
            IdEstabelecimento = 2
        });
        contexto.SaveChanges();

        // Act
        var horarios = servicoHorario.GetByProfissional(1, 1).ToList();

        // Assert
        Assert.HasCount(2, horarios);
        Assert.IsTrue(horarios.All(h => h.IdEstabelecimento == 1));
        Assert.IsFalse(horarios.Any(h => h.Id == 4));
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoDiaExisteEmOutroEstabelecimento_DeveRetornarColecaoVazia()
    {
        // Act (Segunda-feira existe para Profissional 1 no Estabelecimento 1, mas consulta Estabelecimento 2)
        var horarios = servicoHorario.GetByDiaSemana("Segunda-feira", 1, 2).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void Create_QuandoHorarioSobrepoeNoMesmoEstabelecimento_DeveLancarException()
    {
        // Arrange (Horário 1 já ocupa Segunda-feira 09:00 - 17:00 para o Profissional 1)
        var horarioConflitante = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(14, 0, 0),
            FimTurno = new TimeSpan(18, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act & Assert
        try
        {
            servicoHorario.Create(horarioConflitante);
            Assert.Fail("Deveria ter lançado InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
            // Exceção esperada lançada com sucesso
        }
    }

    [TestMethod]
    public void Create_QuandoHorarioSobrepoeEmEstabelecimentoDiferente_DeveLancarException()
    {
        // Arrange (Horário 1 já ocupa Segunda-feira 09:00 - 17:00 para Profissional 1 no Estabelecimento 1)
        // Tentativa de criar para o mesmo Profissional 1 no Estabelecimento 2 em horário conflitante
        var horarioConflitante = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(10, 0, 0),
            FimTurno = new TimeSpan(12, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = 2
        };

        // Act & Assert
        try
        {
            servicoHorario.Create(horarioConflitante);
            Assert.Fail("Deveria ter lançado InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
            // Exceção esperada lançada com sucesso
        }
    }

    [TestMethod]
    public void Create_QuandoHorarioNoMesmoDiaNaoSobrepoeTurno_DeveCriarComSucesso()
    {
        // Arrange (Horário 1 é Segunda-feira 09:00 - 17:00; novo é 17:30 - 21:00)
        var horarioNovo = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(17, 30, 0),
            FimTurno = new TimeSpan(21, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act
        var id = servicoHorario.Create(horarioNovo);

        // Assert
        Assert.AreEqual(4u, id);
        Assert.IsNotNull(servicoHorario.Get(4, 1));
    }

    [TestMethod]
    public void Create_QuandoHorarioComMesmoTurnoEmPeriodosDeVigenciaDiferentes_DeveCriarComSucesso()
    {
        // Arrange (Atualiza vigência do horário 1 para encerrar em 31/12/2023)
        var h1 = servicoHorario.Get(1, 1);
        Assert.IsNotNull(h1);
        h1.InicioVigencia = new DateTime(2023, 1, 1);
        h1.FimVigencia = new DateTime(2023, 12, 31);
        servicoHorario.Edit(h1, 1);

        // Novo horário na mesma Segunda-feira das 09:00 às 17:00, mas com vigência a partir de 2024
        var horarioNovo = new Horario
        {
            Id = 4,
            InicioTurno = new TimeSpan(9, 0, 0),
            FimTurno = new TimeSpan(17, 0, 0),
            InicioVigencia = new DateTime(2024, 1, 1),
            FimVigencia = null,
            DiaSemana = "Segunda-feira",
            IdProfissional = 1,
            IdEstabelecimento = 1
        };

        // Act
        var id = servicoHorario.Create(horarioNovo);

        // Assert
        Assert.AreEqual(4u, id);
    }

    [TestMethod]
    public void Edit_QuandoAtualizacaoCausaSobreposicaoComOutroHorario_DeveLancarException()
    {
        // Arrange (Horário 1: Segunda-feira 09:00 - 17:00, Horário 2: Terça-feira 09:00 - 17:00)
        // Editando Horário 2 para Segunda-feira no mesmo intervalo do Horário 1
        var horario2 = servicoHorario.Get(2, 1);
        Assert.IsNotNull(horario2);

        horario2.DiaSemana = "Segunda-feira";
        horario2.InicioTurno = new TimeSpan(10, 0, 0);
        horario2.FimTurno = new TimeSpan(16, 0, 0);

        // Act & Assert
        try
        {
            servicoHorario.Edit(horario2, 1);
            Assert.Fail("Deveria ter lançado InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
            // Exceção esperada lançada com sucesso
        }
    }

    [TestMethod]
    public void Edit_QuandoAtualizaProprioHorarioSemSobreposicaoComOutros_DeveAtualizarComSucesso()
    {
        // Arrange (Horário 1 sendo ajustado de 09:00-17:00 para 08:30-16:30)
        var horario1 = servicoHorario.Get(1, 1);
        Assert.IsNotNull(horario1);

        horario1.InicioTurno = new TimeSpan(8, 30, 0);
        horario1.FimTurno = new TimeSpan(16, 30, 0);

        // Act
        servicoHorario.Edit(horario1, 1);

        // Assert
        var atualizado = servicoHorario.Get(1, 1);
        Assert.IsNotNull(atualizado);
        Assert.AreEqual(new TimeSpan(8, 30, 0), atualizado.InicioTurno);
        Assert.AreEqual(new TimeSpan(16, 30, 0), atualizado.FimTurno);
    }
}
