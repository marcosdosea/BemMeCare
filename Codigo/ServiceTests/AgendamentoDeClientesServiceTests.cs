using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class AgendamentoDeClientesServiceTests
{
    private BemMeCareContext contexto = null!;
    private IAgendamentoDeClientesService agendamentoService = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase($"BemMeCare_{Guid.NewGuid()}")
            .Options;

        contexto = new BemMeCareContext(opcoes);
        contexto.Pessoas.AddRange(CriarPessoa(1), CriarPessoa(5), CriarPessoa(20), CriarPessoa(21));
        contexto.Profissionals.Add(new Profissional
        {
            IdPessoa = 1,
            IdEstabelecimento = 1,
            EstaAtivo = 1
        });
        contexto.Horarios.AddRange(
            CriarHorario("Segunda-feira"),
            CriarHorario("Terça-feira"),
            CriarHorario("Quarta-feira"),
            CriarHorario("Quinta-feira"),
            CriarHorario("Sexta-feira"),
            CriarHorario("Sábado"),
            CriarHorario("Domingo"));
        contexto.Servicoprofissionals.AddRange(
            CriarServicoProfissional(10, 100m, 30, 20, "TODO_AGENDAMENTO"),
            CriarServicoProfissional(11, 50m, 15, 50, "PRIMEIRO_AGENDAMENTO"));
        contexto.SaveChanges();

        agendamentoService = new AgendamentoDeClientesService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveCalcularPrecoFinal()
    {
        var dataHora = new DateTime(2026, 12, 1, 10, 0, 0);
        var agendamento = CriarAgendamento(dataHora);

        var id = agendamentoService.Create(agendamento, [10, 11]);

        var agendamentoCriado = contexto.Agendamentos
            .Single(item => item.Id == id);
        Assert.AreEqual(150m, agendamentoCriado.PrecoFinal);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveCalcularDuracaoEFim()
    {
        var dataHora = new DateTime(2026, 12, 1, 10, 0, 0);
        var agendamento = CriarAgendamento(dataHora);

        var id = agendamentoService.Create(agendamento, [10, 11]);

        var agendamentoCriado = contexto.Agendamentos
            .Single(item => item.Id == id);
        Assert.AreEqual(
            (45u, dataHora.AddMinutes(45)),
            (agendamentoCriado.DuracaoMinutos, agendamentoCriado.DataHoraFimAtendimento));
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveDefinirStatusIniciais()
    {
        var agendamento = CriarAgendamento(new DateTime(2026, 12, 1, 10, 0, 0));

        agendamentoService.Create(agendamento, [10, 11]);

        Assert.AreEqual(
            ("PE", "MA", 0m),
            (agendamento.StatusAgendamento, agendamento.StatusPagamento, agendamento.ValorPagoSinal));
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRegistrarPrecoCobradoDosServicos()
    {
        var agendamento = CriarAgendamento(new DateTime(2026, 12, 1, 10, 0, 0));

        var id = agendamentoService.Create(agendamento, [10, 11]);

        var servicosDoAgendamento = contexto.Agendamentoservicos
            .Where(item => item.IdAgendamento == id)
            .ToList();
        CollectionAssert.AreEqual(
            new[] { 100m, 50m },
            servicosDoAgendamento
                .OrderBy(item => item.IdServico)
                .Select(item => item.PrecoCobrado)
                .ToArray());
    }

    [TestMethod]
    public void Create_QuandoServicoNaoPertenceAoProfissional_DeveFalharSemSalvar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Create(agendamento, [999]));

        Assert.IsFalse(contexto.Agendamentos.Any());
    }

    [TestMethod]
    public void Create_QuandoNenhumServicoFoiSelecionado_DeveFalhar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Create(agendamento, []));
    }

    [TestMethod]
    public void Create_QuandoProfissionalEstaInativo_DeveFalhar()
    {
        contexto.Profissionals.Single().EstaAtivo = 0;
        contexto.SaveChanges();
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Create(agendamento, [10]));
    }

    [TestMethod]
    public void Create_QuandoHorarioSobrepoeOutroAgendamento_DeveFalharSemSalvar()
    {
        var dataHora = new DateTime(2026, 12, 1, 10, 15, 0);
        var existente = CriarAgendamento(dataHora);
        existente.DuracaoMinutos = 30;
        existente.DataHoraFimAtendimento = dataHora.AddMinutes(30);
        contexto.Agendamentos.Add(existente);
        contexto.SaveChanges();
        var novoAgendamento = CriarAgendamento(dataHora.AddMinutes(-15));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Create(novoAgendamento, [10]));
        Assert.AreEqual(1, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void Create_QuandoHorarioAdjacenteAOutroAgendamento_DevePermitirCriacao()
    {
        var dataHora = new DateTime(2026, 12, 1, 10, 45, 0);
        var existente = CriarAgendamento(dataHora);
        existente.DuracaoMinutos = 30;
        existente.DataHoraFimAtendimento = dataHora.AddMinutes(30);
        contexto.Agendamentos.Add(existente);
        contexto.SaveChanges();
        var novoAgendamento = CriarAgendamento(dataHora.AddMinutes(-45));

        agendamentoService.Create(novoAgendamento, [10, 11]);

        Assert.AreEqual(2, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void Create_QuandoHorarioAnteriorEstaCancelado_DevePermitirCriacao()
    {
        var dataHora = new DateTime(2026, 12, 1, 10, 15, 0);
        var existente = CriarAgendamento(dataHora);
        existente.DuracaoMinutos = 30;
        existente.DataHoraFimAtendimento = dataHora.AddMinutes(30);
        existente.StatusAgendamento = "CA";
        existente.StatusPagamento = "CA";
        contexto.Agendamentos.Add(existente);
        contexto.SaveChanges();
        var novoAgendamento = CriarAgendamento(dataHora.AddMinutes(-15));

        agendamentoService.Create(novoAgendamento, [10]);

        Assert.AreEqual(2, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void Create_QuandoHorarioEstaForaDoExpediente_DeveFalharSemSalvar()
    {
        var agendamento = CriarAgendamento(new DateTime(2026, 12, 1, 7, 30, 0));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Create(agendamento, [10]));
        Assert.AreEqual(0, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void GetByData_QuandoInformadoProfissional_DeveFiltrarPorDiaEstabelecimentoEProfissional()
    {
        var data = new DateTime(2026, 12, 1, 9, 0, 0);
        var doDia = CriarAgendamento(data);
        doDia.IdPessoaCliente = 20;
        var outroProfissional = CriarAgendamento(data.AddHours(1));
        outroProfissional.IdProfissional = 2;
        var outroDia = CriarAgendamento(data.AddDays(1));
        outroDia.IdPessoaCliente = 21;
        contexto.Agendamentos.AddRange(doDia, outroProfissional, outroDia);
        contexto.SaveChanges();

        var resultado = agendamentoService.GetByData(data, 1, 1).ToList();

        CollectionAssert.AreEqual(new[] { doDia.Id }, resultado.Select(item => item.Id).ToArray());
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoExiste_DeveAtualizarOsStatus()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.StatusAgendamento = "PE";
        agendamento.StatusPagamento = "MA";
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        agendamentoService.Cancel(agendamento.Id, 1);

        CollectionAssert.AreEqual(
            new[] { "CA", "CA" },
            new[] { agendamento.StatusAgendamento, agendamento.StatusPagamento });
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoExiste_DevePreservarAgendamentoEServicos()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.ValorPagoSinal = 20m;
        agendamento.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 10,
            PrecoCobrado = 100m
        });
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        agendamentoService.Cancel(agendamento.Id, 1);

        var agendamentoCancelado = contexto.Agendamentos
            .Include(item => item.Agendamentoservicos)
            .Single(item => item.Id == agendamento.Id);
        Assert.AreEqual(
            ("CA", 20m, 1),
            (
                agendamentoCancelado.StatusAgendamento,
                agendamentoCancelado.ValorPagoSinal,
                agendamentoCancelado.Agendamentoservicos.Count));
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoPertenceAOutroEstabelecimento_NaoDeveAlterarStatus()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        agendamentoService.Cancel(agendamento.Id, 2);

        Assert.AreEqual("PE", agendamento.StatusAgendamento);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoEstaConcluido_DeveFalhar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(-1));
        agendamento.StatusAgendamento = "CO";
        agendamento.StatusPagamento = "FI";
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.Cancel(agendamento.Id, 1));
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoPoliticaEhTodoAgendamento_DeveRegistrarValorESinalPago()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 10,
            PrecoCobrado = 100m
        });
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        agendamentoService.ConfirmarSinal(agendamento.Id, 1);

        Assert.AreEqual(
            (20m, "SP", "MA"),
            (agendamento.ValorPagoSinal, agendamento.StatusAgendamento, agendamento.StatusPagamento));
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoPrecoDoServicoMudou_DeveUsarPrecoRegistradoNoAgendamento()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 10,
            PrecoCobrado = 100m
        });
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();
        contexto.Servicoprofissionals.Single(item => item.IdServico == 10).Preco = 200m;
        contexto.SaveChanges();

        agendamentoService.ConfirmarSinal(agendamento.Id, 1);

        Assert.AreEqual(20m, agendamento.ValorPagoSinal);
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoAgendamentoEstaCancelado_DeveFalhar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.StatusAgendamento = "CA";
        agendamento.StatusPagamento = "CA";
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.ConfirmarSinal(agendamento.Id, 1));
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoAgendamentoJaTemSinalPago_DeveFalhar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(1));
        agendamento.StatusAgendamento = "SP";
        agendamento.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 10,
            PrecoCobrado = 100m
        });
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.ConfirmarSinal(agendamento.Id, 1));
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoAgendamentoEstaConcluido_DeveFalhar()
    {
        var agendamento = CriarAgendamento(DateTime.Today.AddDays(-1));
        agendamento.StatusAgendamento = "CO";
        agendamento.StatusPagamento = "FI";
        contexto.Agendamentos.Add(agendamento);
        contexto.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.ConfirmarSinal(agendamento.Id, 1));
    }

    [TestMethod]
    public void ConfirmarSinal_QuandoEhSegundoAgendamentoDeServicoComPoliticaPrimeiro_DeveRejeitar()
    {
        var anterior = CriarAgendamento(DateTime.Today.AddDays(-2));
        anterior.IdPessoaCliente = 5;
        anterior.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 11,
            PrecoCobrado = 50m
        });
        var atual = CriarAgendamento(DateTime.Today.AddDays(1));
        atual.IdPessoaCliente = 5;
        atual.Agendamentoservicos.Add(new Agendamentoservico
        {
            IdServico = 11,
            PrecoCobrado = 50m
        });
        contexto.Agendamentos.AddRange(anterior, atual);
        contexto.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => agendamentoService.ConfirmarSinal(atual.Id, 1));
        Assert.AreEqual(0m, atual.ValorPagoSinal);
        Assert.AreEqual("PE", atual.StatusAgendamento);
    }

    private static Agendamento CriarAgendamento(DateTime dataHora)
    {
        return new Agendamento
        {
            DataHoraAtendimento = dataHora,
            DataHoraCriacao = DateTime.Now,
            IdPessoaCliente = 5,
            IdPessoaMarcou = 1,
            IdProfissional = 1,
            IdEstabelecimento = 1,
            StatusAgendamento = "PE",
            StatusPagamento = "MA"
        };
    }

    private static Pessoa CriarPessoa(uint id)
    {
        return new Pessoa
        {
            Id = id,
            Nome = $"Pessoa {id}",
            Email = $"pessoa{id}@teste.com",
            Senha = "senha",
            Cpf = id.ToString().PadLeft(11, '0'),
            Telefone = id.ToString().PadLeft(11, '0'),
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = string.Empty,
            Genero = string.Empty,
            Sexualidade = string.Empty,
            Cep = string.Empty,
            Rua = string.Empty,
            Bairro = string.Empty,
            Numero = string.Empty,
            Complemento = string.Empty,
            Cidade = string.Empty,
            Estado = string.Empty
        };
    }

    private static Horario CriarHorario(string diaSemana)
    {
        return new Horario
        {
            InicioTurno = new TimeSpan(8, 0, 0),
            FimTurno = new TimeSpan(20, 0, 0),
            InicioVigencia = new DateTime(2020, 1, 1),
            FimVigencia = null,
            DiaSemana = diaSemana,
            IdProfissional = 1,
            IdEstabelecimento = 1
        };
    }

    private static Servicoprofissional CriarServicoProfissional(
        uint idServico,
        decimal preco,
        uint duracao,
        float percentualSinal,
        string quandoPagarSinal)
    {
        return new Servicoprofissional
        {
            IdProfissional = 1,
            IdEstabelecimento = 1,
            IdServico = idServico,
            Preco = preco,
            Duracao = duracao,
            PercentualSinal = percentualSinal,
            QuandoPagarSinal = quandoPagarSinal,
            ExigeAnamnese = 0,
            FormAnamnese = string.Empty
        };
    }
}
