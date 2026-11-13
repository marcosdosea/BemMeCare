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
        var horarioCriado = servicoHorario.Get(horarioEsperado.Id);

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
        servicoHorario.Delete(2);

        // Assert
        var horarioExcluido = servicoHorario.Get(2);
        var horariosRestantes = servicoHorario.GetAll().ToList();

        Assert.IsNull(horarioExcluido);
        Assert.HasCount(2, horariosRestantes);
        Assert.IsFalse(horariosRestantes.Any(horario => horario.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoHorarioNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var horariosIniciais = servicoHorario.GetAll().ToList();

        // Act
        servicoHorario.Delete(999);

        // Assert
        var horariosFinais = servicoHorario.GetAll().ToList();

        Assert.HasCount(horariosIniciais.Count, horariosFinais);
        CollectionAssert.AreEquivalent(
            horariosIniciais.Select(horario => horario.Id).ToList(),
            horariosFinais.Select(horario => horario.Id).ToList());
    }

    [TestMethod]
    public void Edit_QuandoHorarioExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var horario = servicoHorario.Get(1);

        Assert.IsNotNull(horario);

        horario.InicioTurno = new TimeSpan(10, 0, 0);
        horario.FimTurno = new TimeSpan(18, 0, 0);
        horario.InicioVigencia = new DateTime(2023, 1, 1);
        horario.FimVigencia = new DateTime(2023, 12, 31);
        horario.DiaSemana = "Sábado";
        horario.IdProfissional = 1;
        horario.IdEstabelecimento = 1;

        // Act
        servicoHorario.Edit(horario);

        // Assert
        var horarioAtualizado = servicoHorario.Get(1);

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
        var horario = servicoHorario.Get(1);

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
        var horario = servicoHorario.Get(999);

        // Assert
        Assert.IsNull(horario);
    }

    [TestMethod]
    public void GetAll_QuandoExistemHorarios_DeveRetornarTodosOsHorarios()
    {
        // Act
        var horarios = servicoHorario.GetAll().ToList();

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
        var horarios = servicoHorario.GetAll().ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByProfissional_QuandoExistemHorariosParaOProfissional_DeveRetornarHorariosCorrespondentes()
    {
        // Act
        var horarios = servicoHorario.GetByProfissional(1).ToList();

        // Assert
        Assert.HasCount(2, horarios);
        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2 },
            horarios.Select(h => h.Id).ToList());
        Assert.IsTrue(horarios.All(h => h.IdProfissional == 1));
    }

    [TestMethod]
    public void GetByProfissional_QuandoNaoExistemHorariosParaOProfissional_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByProfissional(999).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoExisteCorrespondencia_DeveRetornarHorariosCorrespondentes()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Segunda-feira", 1).ToList();

        // Assert
        Assert.HasCount(1, horarios);
        Assert.AreEqual(1u, horarios[0].Id);
        Assert.AreEqual("Segunda-feira", horarios[0].DiaSemana);
        Assert.AreEqual(1u, horarios[0].IdProfissional);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoDiaExisteMasProfissionalDiferente_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Quarta-feira", 1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }

    [TestMethod]
    public void GetByDiaSemana_QuandoProfissionalExisteMasDiaDiferente_DeveRetornarColecaoVazia()
    {
        // Act
        var horarios = servicoHorario.GetByDiaSemana("Domingo", 1).ToList();

        // Assert
        Assert.IsNotNull(horarios);
        Assert.IsEmpty(horarios);
    }
}
