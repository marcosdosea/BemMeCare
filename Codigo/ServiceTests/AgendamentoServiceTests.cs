using Core;
using Core.DTO;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class AgendamentoServiceTests
{
    private const uint IdProfissionalA = 2;
    private const uint IdProfissionalB = 3;
    private const uint IdProfissionalInativo = 4;
    private const uint IdProfissionalOutroEstabelecimento = 5;

    private const uint IdServicoCorte = 1;
    private const uint IdServicoBarba = 2;
    private const uint IdServicoManicure = 3;
    private const uint IdServicoOutroEstabelecimento = 4;

    private const uint IdCliente = 1;

    private const uint IdOutroCliente = 6;

    private static readonly DateOnly DataFutura = ObterProximaSegundaFeira();

    private uint proximoIdAgendamento = 1;

    private BemMeCareContext contexto = null!;
    private IAgendamentoService servico = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase($"BemMeCare_{Guid.NewGuid()}")
            .Options;

        contexto = new BemMeCareContext(opcoes);
        contexto.Pessoas.AddRange(ObterPessoasDeTeste());
        contexto.Profissionals.AddRange(ObterProfissionaisDeTeste());
        contexto.Servicos.AddRange(ObterServicosDeTeste());
        contexto.Servicoprofissionals.AddRange(ObterServicosProfissionaisDeTeste());
        contexto.SaveChanges();

        servico = new AgendamentoService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoSelecaoEhValida_DeveRetornarVerdadeiro()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoCorte);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoVariosServicosDoMesmoProfissional_DeveRetornarVerdadeiro()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoCorte, IdServicoBarba);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoListaEstaVazia_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalA);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoHaServicosDuplicados_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoCorte, IdServicoCorte);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoProfissionalNaoRealizaServico_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalB, IdServicoCorte, IdServicoBarba);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoServicoNaoExiste_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoCorte, 999);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoProfissionalNaoExiste_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(999, IdServicoCorte);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoProfissionalEstaInativo_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalInativo, IdServicoCorte);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoProfissionalPertenceAOutroEstabelecimento_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalOutroEstabelecimento, IdServicoOutroEstabelecimento);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void ValidateProfissionalServicosAgendamento_QuandoServicoVinculadoAOutroEstabelecimento_DeveRetornarFalso()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoOutroEstabelecimento);

        var resultado = servico.ValidateProfissionalServicosAgendamento(selecao, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoNaoHaAgendamentos_DeveEncadearBlocosDesdeOInicioDoTurno()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30", "09:00", "09:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoHaAgendamentoNoMeioDoTurno_DeveGerarBlocosAntesEDepois()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "09:30", "10:30");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte, IdServicoBarba);
        var turnos = new[] { CriarTurno("08:00", "12:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "10:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentoTerminaNoInicioDeUmBloco_DeveOferecerBlocoColado()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "08:00", "09:00");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("09:00", "09:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentoEstaCancelado_NaoDeveBloquearHorario()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00", "CA");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30", "09:00", "09:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoHaDoisTurnosComPausa_DeveRetornarBlocosDeAmbosOrdenados()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("14:00", "15:00"), CriarTurno("08:00", "09:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30", "14:00", "14:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoTurnosSaoContiguos_NaoDeveGerarBlocoQueAtravesseOsTurnos()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte, IdServicoBarba);
        var turnos = new[] { CriarTurno("08:00", "09:30"), CriarTurno("09:30", "11:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "09:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoDataEstaNoPassado_DeveRetornarColecaoVazia()
    {
        var ontem = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        var selecao = CriarSelecaoComData(ontem, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        Assert.IsEmpty(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoDataEhNula_DeveRetornarColecaoVazia()
    {
        var selecao = CriarSelecaoComData(null, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        Assert.IsEmpty(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoNaoHaServicosSelecionados_DeveRetornarColecaoVazia()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        Assert.IsEmpty(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoNaoHaTurnos_DeveRetornarColecaoVazia()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, [], 1).ToList();

        Assert.IsEmpty(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentoEhDeOutroProfissional_NaoDeveInterferir()
    {
        AdicionarAgendamento(IdProfissionalB, 1, "08:00", "10:00");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "09:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentoEhDeOutroEstabelecimento_NaoDeveInterferir()
    {
        AdicionarAgendamento(IdProfissionalA, 2, "08:00", "10:00");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "09:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentoEhDeOutroDia_NaoDeveInterferir()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "08:00", "10:00", data: DataFutura.AddDays(1));
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "09:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoDuracaoEhMaiorQueOTurno_DeveRetornarColecaoVazia()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte, IdServicoBarba);
        var turnos = new[] { CriarTurno("08:00", "08:30") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        Assert.IsEmpty(resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoHaVariosServicos_DeveUsarASomaDasDuracoes()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte, IdServicoBarba);
        var turnos = new[] { CriarTurno("08:00", "10:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "09:00"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoDuracaoVariaPorProfissional_DeveUsarADuracaoDoProfissional()
    {
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalB, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "10:15", IdProfissionalB) };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:45", "09:30"), resultado);
    }

    [TestMethod]
    public void GetHorariosDisponiveisProfissionalAgendamento_QuandoAgendamentosSeSobrepoem_DeveCalcularIntervalosLivresCorretamente()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00");
        AdicionarAgendamento(IdProfissionalA, 1, "09:30", "10:30");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "12:00") };

        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30", "10:30", "11:00", "11:30"), resultado);
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoSelecaoEhValida_DeveRetornarResumoCompleto()
    {
        var selecao = CriarSelecaoComHorario("09:30", IdProfissionalA, IdServicoCorte, IdServicoBarba);

        var resultado = servico.GetResumoAgendamento(selecao, 1);

        Assert.AreEqual("Profissional A", resultado.NomeProfissional);
        Assert.AreEqual(85m, resultado.PrecoTotal);
        Assert.AreEqual(60u, resultado.DuracaoTotalEmMinutos);
        Assert.AreEqual(TimeOnly.Parse("09:30"), resultado.HorarioAtendimento);
        Assert.HasCount(2, resultado.Servicos);
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoSelecaoEhValida_DeveRetornarDadosDeCadaServico()
    {
        var selecao = CriarSelecaoComHorario("09:30", IdProfissionalA, IdServicoCorte);

        var resultado = servico.GetResumoAgendamento(selecao, 1);
        var item = resultado.Servicos.Single();

        Assert.AreEqual(IdServicoCorte, item.Id);
        Assert.AreEqual("Corte", item.Nome);
        Assert.AreEqual(45m, item.Preco);
        Assert.AreEqual(30u, item.DuracaoEmMinutos);
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoHaUmUnicoServico_DeveCalcularTotaisDesseServico()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalA, IdServicoBarba);

        var resultado = servico.GetResumoAgendamento(selecao, 1);

        Assert.AreEqual(40m, resultado.PrecoTotal);
        Assert.AreEqual(30u, resultado.DuracaoTotalEmMinutos);
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoPrecoVariaPorProfissional_DeveUsarPrecoEDuracaoDoProfissional()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalB, IdServicoCorte);

        var resultado = servico.GetResumoAgendamento(selecao, 1);

        Assert.AreEqual("Profissional B", resultado.NomeProfissional);
        Assert.AreEqual(60m, resultado.PrecoTotal);
        Assert.AreEqual(45u, resultado.DuracaoTotalEmMinutos);
        Assert.AreEqual(60m, resultado.Servicos.Single().Preco);
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoHaVariosServicos_DeveOrdenarServicosPorNome()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalA, IdServicoCorte, IdServicoBarba);

        var resultado = servico.GetResumoAgendamento(selecao, 1);

        CollectionAssert.AreEqual(
            new[] { "Barba", "Corte" },
            resultado.Servicos.Select(s => s.Nome).ToList());
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoHorarioEhNulo_DeveLancarExcecao()
    {
        var selecao = CriarSelecao(IdProfissionalA, IdServicoCorte);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoProfissionalNaoRealizaServico_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalB, IdServicoCorte, IdServicoBarba);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoServicoNaoExiste_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalA, IdServicoCorte, 999);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoProfissionalNaoExiste_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", 999, IdServicoCorte);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoProfissionalEstaInativo_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalInativo, IdServicoCorte);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoProfissionalPertenceAOutroEstabelecimento_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalOutroEstabelecimento, IdServicoOutroEstabelecimento);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void GetResumoAgendamento_QuandoServicoVinculadoAOutroEstabelecimento_DeveLancarExcecao()
    {
        var selecao = CriarSelecaoComHorario("08:00", IdProfissionalA, IdServicoCorte, IdServicoOutroEstabelecimento);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => servico.GetResumoAgendamento(selecao, 1));
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoAgendamentoGravado()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA, IdServicoCorte);

        var id = servico.Create(selecao, 1, IdCliente);

        Assert.IsNotNull(contexto.Agendamentos.SingleOrDefault(a => a.Id == id));
        Assert.AreEqual(1, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveGravarPrecoDuracaoInicioEFimRecalculados()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA, IdServicoCorte, IdServicoBarba);

        var id = servico.Create(selecao, 1, IdCliente);

        var agendamento = ObterAgendamento(id);
        var inicioEsperado = DataFutura.ToDateTime(TimeOnly.Parse("09:30"));
        Assert.AreEqual(85m, agendamento.PrecoFinal);
        Assert.AreEqual(60u, agendamento.DuracaoMinutos);
        Assert.AreEqual(inicioEsperado, agendamento.DataHoraAtendimento);
        Assert.AreEqual(inicioEsperado.AddMinutes(60), agendamento.DataHoraFimAtendimento);
    }

    [TestMethod]
    public void Create_QuandoPrecoVariaPorProfissional_DeveGravarValoresDoProfissional()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "08:00", IdProfissionalB, IdServicoCorte);

        var id = servico.Create(selecao, 1, IdCliente);

        var agendamento = ObterAgendamento(id);
        Assert.AreEqual(60m, agendamento.PrecoFinal);
        Assert.AreEqual(45u, agendamento.DuracaoMinutos);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveGravarUmItemPorServicoComPrecoCobrado()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA, IdServicoCorte, IdServicoBarba);

        var id = servico.Create(selecao, 1, IdCliente);

        var itens = ObterAgendamento(id).Agendamentoservicos.OrderBy(i => i.IdServico).ToList();
        Assert.HasCount(2, itens);
        Assert.AreEqual(IdServicoCorte, itens[0].IdServico);
        Assert.AreEqual(45m, itens[0].PrecoCobrado);
        Assert.AreEqual(IdServicoBarba, itens[1].IdServico);
        Assert.AreEqual(40m, itens[1].PrecoCobrado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveGravarStatusClienteProfissionalEEstabelecimento()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA, IdServicoCorte);

        var id = servico.Create(selecao, 1, IdOutroCliente);

        var agendamento = ObterAgendamento(id);
        Assert.AreEqual("PE", agendamento.StatusAgendamento);
        Assert.AreEqual("MA", agendamento.StatusPagamento);
        Assert.AreEqual(0m, agendamento.ValorPagoSinal);
        Assert.AreEqual(IdOutroCliente, agendamento.IdPessoaCliente);
        Assert.AreEqual(IdOutroCliente, agendamento.IdPessoaMarcou);
        Assert.AreEqual(IdProfissionalA, agendamento.IdProfissional);
        Assert.AreEqual(1u, agendamento.IdEstabelecimento);
        Assert.IsNull(agendamento.Observacoes);
    }

    [TestMethod]
    public void Create_QuandoHorarioJaEstaOcupado_NaoDeveRejeitarOAgendamento()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "09:30", "10:30");
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA, IdServicoCorte);

        servico.Create(selecao, 1, IdCliente);

        Assert.AreEqual(2, contexto.Agendamentos.Count());
    }

    [TestMethod]
    public void Create_QuandoDataEhNula_DeveLancarExcecaoENaoGravar()
    {
        var selecao = CriarSelecaoCompleta(null, "09:30", IdProfissionalA, IdServicoCorte);

        Assert.ThrowsExactly<Exception>(() => servico.Create(selecao, 1, IdCliente));
        Assert.IsEmpty(contexto.Agendamentos.ToList());
    }

    [TestMethod]
    public void Create_QuandoHorarioEhNulo_DeveLancarExcecaoENaoGravar()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, null, IdProfissionalA, IdServicoCorte);

        Assert.ThrowsExactly<Exception>(() => servico.Create(selecao, 1, IdCliente));
        Assert.IsEmpty(contexto.Agendamentos.ToList());
    }

    [TestMethod]
    public void Create_QuandoListaDeServicosEstaVazia_DeveLancarExcecaoENaoGravar()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalA);

        Assert.ThrowsExactly<Exception>(() => servico.Create(selecao, 1, IdCliente));
        Assert.IsEmpty(contexto.Agendamentos.ToList());
    }

    [TestMethod]
    public void Create_QuandoProfissionalNaoRealizaServico_DeveLancarExcecaoENaoGravar()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalB, IdServicoBarba);

        Assert.ThrowsExactly<Exception>(() => servico.Create(selecao, 1, IdCliente));
        Assert.IsEmpty(contexto.Agendamentos.ToList());
    }

    [TestMethod]
    public void Create_QuandoProfissionalPertenceAOutroEstabelecimento_DeveLancarExcecaoENaoGravar()
    {
        var selecao = CriarSelecaoCompleta(DataFutura, "09:30", IdProfissionalOutroEstabelecimento, IdServicoOutroEstabelecimento);

        Assert.ThrowsExactly<Exception>(() => servico.Create(selecao, 1, IdCliente));
        Assert.IsEmpty(contexto.Agendamentos.ToList());
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoPodeSerCancelado_DeveRetornarVerdadeiroEAtualizarStatus()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00");

        var resultado = servico.Cancel(id, 1);

        var agendamento = ObterAgendamento(id);
        Assert.IsTrue(resultado);
        Assert.AreEqual("CA", agendamento.StatusAgendamento);
        Assert.AreEqual("CA", agendamento.StatusPagamento);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoEhCancelado_DeveLiberarOHorario()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 1, "08:00", "09:00");
        var selecao = CriarSelecaoComData(DataFutura, IdProfissionalA, IdServicoCorte);
        var turnos = new[] { CriarTurno("08:00", "09:00") };

        servico.Cancel(id, 1);
        var resultado = servico.GetHorariosDisponiveisProfissionalAgendamento(selecao, turnos, 1).ToList();

        CollectionAssert.AreEqual(Horas("08:00", "08:30"), resultado);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoNaoExiste_DeveRetornarFalso()
    {
        var resultado = servico.Cancel(999, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoPertenceAOutroEstabelecimento_DeveRetornarFalsoENaoAlterar()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 2, "09:00", "10:00");

        var resultado = servico.Cancel(id, 1);

        var agendamento = ObterAgendamento(id);
        Assert.IsFalse(resultado);
        Assert.AreEqual("PE", agendamento.StatusAgendamento);
        Assert.AreEqual("MA", agendamento.StatusPagamento);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoJaEstaCancelado_DeveRetornarFalso()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00", statusAgendamento: "CA", statusPagamento: "CA");

        var resultado = servico.Cancel(id, 1);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Cancel_QuandoAgendamentoEstaConcluido_DeveRetornarFalsoENaoAlterar()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00", statusAgendamento: "CO", statusPagamento: "FI");

        var resultado = servico.Cancel(id, 1);

        var agendamento = ObterAgendamento(id);
        Assert.IsFalse(resultado);
        Assert.AreEqual("CO", agendamento.StatusAgendamento);
        Assert.AreEqual("FI", agendamento.StatusPagamento);
    }

    [TestMethod]
    public void GetAll_QuandoClienteTemAgendamentos_DeveRetornarSomenteOsDoClienteNoEstabelecimento()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "08:00", "09:00");
        AdicionarAgendamento(IdProfissionalA, 1, "09:00", "10:00");
        AdicionarAgendamento(IdProfissionalA, 1, "10:00", "11:00", idCliente: IdOutroCliente);
        AdicionarAgendamento(IdProfissionalOutroEstabelecimento, 2, "08:00", "09:00");

        var resultados = servico.GetAll(IdCliente, 1).ToList();

        Assert.HasCount(2, resultados);
    }

    [TestMethod]
    public void GetAll_QuandoClienteNaoTemAgendamentos_DeveRetornarColecaoVazia()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "08:00", "09:00", idCliente: IdOutroCliente);

        var resultados = servico.GetAll(IdCliente, 1).ToList();

        Assert.IsEmpty(resultados);
    }

    [TestMethod]
    public void GetAll_QuandoEstabelecimentoNaoPossuiAgendamentos_DeveRetornarColecaoVazia()
    {
        AdicionarAgendamento(IdProfissionalA, 1, "08:00", "09:00");

        var resultados = servico.GetAll(IdCliente, 999).ToList();

        Assert.IsEmpty(resultados);
    }

    [TestMethod]
    public void GetAll_QuandoAgendamentoTemServicos_DeveProjetarOsDadosCorretamente()
    {
        var id = AdicionarAgendamento(IdProfissionalA, 1, "09:30", "10:30", precoFinal: 85m);
        AdicionarServicoAoAgendamento(id, IdServicoCorte, 45m);
        AdicionarServicoAoAgendamento(id, IdServicoBarba, 40m);

        var resultado = servico.GetAll(IdCliente, 1).Single();

        Assert.AreEqual("Profissional A", resultado.NomeProfissional);
        Assert.AreEqual(DataFutura.ToDateTime(TimeOnly.Parse("09:30")), resultado.HorarioAtendimento);
        Assert.AreEqual(85m, resultado.PrecoTotal);
        Assert.AreEqual(60u, resultado.DuracaoTotalEmMinutos);
        CollectionAssert.AreEquivalent(
            new[] { "Corte", "Barba" },
            resultado.Servicos.ToList());
    }

    private static SelecaoAgendamentoDTO CriarSelecao(uint idProfissional, params uint[] idsServicos)
    {
        return new SelecaoAgendamentoDTO
        {
            IdProfissional = idProfissional,
            IdServicosSelecionados = idsServicos.ToList()
        };
    }

    private static Pessoa CriarPessoa(uint id, string nome)
    {
        return new Pessoa
        {
            Id = id,
            Nome = nome,
            Email = $"pessoa{id}@teste.com",
            Senha = "senha",
            Cpf = "00000000000",
            Telefone = "00000000000",
            DataDeNascimento = new DateTime(1990, 1, 1),
            Cor = "N/A",
            Genero = "N/A",
            Sexualidade = "N/A",
            Cep = "00000000",
            Rua = "Rua Teste",
            Bairro = "Bairro Teste",
            Numero = "1",
            Complemento = "N/A",
            Cidade = "Cidade Teste",
            Estado = "XX"
        };
    }

    private static Profissional CriarProfissional(uint idPessoa, uint idEstabelecimento, bool ativo = true)
    {
        return new Profissional
        {
            IdPessoa = idPessoa,
            IdEstabelecimento = idEstabelecimento,
            EstaAtivo = (sbyte)(ativo ? 1 : 0)
        };
    }

    private static Servicoprofissional CriarServicoProfissional(
        uint idProfissional, uint idEstabelecimento, uint idServico, decimal preco, uint duracao)
    {
        return new Servicoprofissional
        {
            IdProfissional = idProfissional,
            IdEstabelecimento = idEstabelecimento,
            IdServico = idServico,
            Preco = preco,
            Duracao = duracao,
            FormAnamnese = string.Empty,
            QuandoPagarSinal = string.Empty
        };
    }

    private static IEnumerable<Pessoa> ObterPessoasDeTeste()
    {
        return
        [
            CriarPessoa(1, "Cliente"),
            CriarPessoa(IdProfissionalA, "Profissional A"),
            CriarPessoa(IdProfissionalB, "Profissional B"),
            CriarPessoa(IdProfissionalInativo, "Profissional Inativo"),
            CriarPessoa(IdProfissionalOutroEstabelecimento, "Profissional Outro Estabelecimento")
        ];
    }

    private static IEnumerable<Profissional> ObterProfissionaisDeTeste()
    {
        return
        [
            CriarProfissional(IdProfissionalA, 1),
            CriarProfissional(IdProfissionalB, 1),
            CriarProfissional(IdProfissionalInativo, 1, ativo: false),
            CriarProfissional(IdProfissionalOutroEstabelecimento, 2)
        ];
    }

    private static IEnumerable<Servico> ObterServicosDeTeste()
    {
        return
        [
            new Servico { Id = IdServicoCorte, Nome = "Corte", IdCategoria = 1, IdEstabelecimento = 1 },
            new Servico { Id = IdServicoBarba, Nome = "Barba", IdCategoria = 1, IdEstabelecimento = 1 },
            new Servico { Id = IdServicoManicure, Nome = "Manicure", IdCategoria = 2, IdEstabelecimento = 1 },
            new Servico { Id = IdServicoOutroEstabelecimento, Nome = "Escova", IdCategoria = 2, IdEstabelecimento = 2 }
        ];
    }

    private static IEnumerable<Servicoprofissional> ObterServicosProfissionaisDeTeste()
    {
        return
        [
            CriarServicoProfissional(IdProfissionalA, 1, IdServicoCorte, 45m, 30),
            CriarServicoProfissional(IdProfissionalA, 1, IdServicoBarba, 40m, 30),
            CriarServicoProfissional(IdProfissionalA, 2, IdServicoOutroEstabelecimento, 70m, 60),
            CriarServicoProfissional(IdProfissionalB, 1, IdServicoCorte, 60m, 45),
            CriarServicoProfissional(IdProfissionalInativo, 1, IdServicoCorte, 50m, 30),
            CriarServicoProfissional(IdProfissionalOutroEstabelecimento, 2, IdServicoOutroEstabelecimento, 80m, 60)
        ];
    }

    private static SelecaoAgendamentoDTO CriarSelecaoComData(DateOnly? data, uint idProfissional, params uint[] idsServicos)
    {
        var selecao = CriarSelecao(idProfissional, idsServicos);
        selecao.DataDoAgendamento = data;

        return selecao;
    }

    private static Horario CriarTurno(string inicio, string fim, uint idProfissional = IdProfissionalA)
    {
        return new Horario
        {
            InicioTurno = TimeSpan.Parse(inicio),
            FimTurno = TimeSpan.Parse(fim),
            DiaSemana = "Segunda-feira",
            InicioVigencia = DateTime.Today.AddYears(-1),
            FimVigencia = null,
            IdProfissional = idProfissional,
            IdEstabelecimento = 1
        };
    }

    private static List<TimeOnly> Horas(params string[] horarios)
    {
        return horarios.Select(TimeOnly.Parse).ToList();
    }

    private static DateOnly ObterProximaSegundaFeira()
    {
        var data = DateOnly.FromDateTime(DateTime.Today).AddDays(7);

        while (data.DayOfWeek != DayOfWeek.Monday)
        {
            data = data.AddDays(1);
        }

        return data;
    }

    private uint AdicionarAgendamento(
    uint idProfissional, uint idEstabelecimento, string inicio, string fim,
    string statusAgendamento = "PE", DateOnly? data = null,
    string statusPagamento = "MA", uint idCliente = IdCliente, decimal precoFinal = 0)
    {
        var dia = (data ?? DataFutura).ToDateTime(TimeOnly.MinValue);
        var dataInicio = dia + TimeSpan.Parse(inicio);
        var dataFim = dia + TimeSpan.Parse(fim);
        var id = proximoIdAgendamento++;

        contexto.Agendamentos.Add(new Agendamento
        {
            Id = id,
            IdEstabelecimento = idEstabelecimento,
            IdProfissional = idProfissional,
            IdPessoaCliente = idCliente,
            IdPessoaMarcou = idCliente,
            DataHoraAtendimento = dataInicio,
            DataHoraFimAtendimento = dataFim,
            DuracaoMinutos = (uint)(dataFim - dataInicio).TotalMinutes,
            PrecoFinal = precoFinal,
            ValorPagoSinal = 0,
            StatusAgendamento = statusAgendamento,
            StatusPagamento = statusPagamento,
            DataHoraCriacao = DateTime.Today
        });
        contexto.SaveChanges();

        return id;
    }
    private static SelecaoAgendamentoDTO CriarSelecaoComHorario(string horario, uint idProfissional, params uint[] idsServicos)
    {
        var selecao = CriarSelecao(idProfissional, idsServicos);
        selecao.HorarioAgendamento = TimeOnly.Parse(horario);

        return selecao;
    }

    private static SelecaoAgendamentoDTO CriarSelecaoCompleta(
    DateOnly? data, string? horario, uint idProfissional, params uint[] idsServicos)
    {
        var selecao = CriarSelecao(idProfissional, idsServicos);
        selecao.DataDoAgendamento = data;
        selecao.HorarioAgendamento = horario is null ? null : TimeOnly.Parse(horario);

        return selecao;
    }

    private Agendamento ObterAgendamento(uint id)
    {
        return contexto.Agendamentos
            .Include(a => a.Agendamentoservicos)
            .Single(a => a.Id == id);
    }

    private void AdicionarServicoAoAgendamento(uint idAgendamento, uint idServico, decimal preco)
    {
        contexto.Set<Agendamentoservico>().Add(new Agendamentoservico
        {
            IdAgendamento = idAgendamento,
            IdServico = idServico,
            PrecoCobrado = preco
        });
        contexto.SaveChanges();
    }
}