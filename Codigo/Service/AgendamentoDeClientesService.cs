using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service;

public class AgendamentoDeClientesService : IAgendamentoDeClientesService
{
    private const string StatusAgendamentoPendente = "PE";
    private const string StatusAgendamentoCancelado = "CA";
    private const string StatusAgendamentoConcluido = "CO";
    private const string StatusAgendamentoSinalPago = "SP";
    private const string StatusPagamentoMarcado = "MA";
    private const string StatusPagamentoCancelado = "CA";
    private const string StatusPagamentoFinalizado = "FI";
    private const string QuandoPagarPrimeiroAgendamento = "PRIMEIRO_AGENDAMENTO";
    private const string QuandoPagarTodoAgendamento = "TODO_AGENDAMENTO";

    private readonly BemMeCareContext context;

    public AgendamentoDeClientesService(BemMeCareContext context)
    {
        this.context = context;
    }

    public IEnumerable<Agendamento> GetByData(
        DateTime data,
        uint idEstabelecimento,
        uint? idProfissional = null)
    {
        var inicioDoDia = data.Date;
        var inicioDoProximoDia = inicioDoDia.AddDays(1);
        var consulta = context.Agendamentos
            .AsNoTracking()
            .Where(agendamento =>
                agendamento.IdEstabelecimento == idEstabelecimento &&
                agendamento.DataHoraAtendimento >= inicioDoDia &&
                agendamento.DataHoraAtendimento < inicioDoProximoDia);

        if (idProfissional.HasValue)
        {
            consulta = consulta.Where(
                agendamento => agendamento.IdProfissional == idProfissional.Value);
        }

        return IncluirDadosRelacionados(consulta).ToList();
    }

    public Agendamento? Get(uint id, uint idEstabelecimento)
    {
        return IncluirDadosRelacionados(context.Agendamentos.AsNoTracking())
            .FirstOrDefault(
                agendamento =>
                    agendamento.Id == id &&
                    agendamento.IdEstabelecimento == idEstabelecimento);
    }

    public uint Create(Agendamento agendamento, IEnumerable<uint> idsServicos)
    {
        ArgumentNullException.ThrowIfNull(agendamento);
        ArgumentNullException.ThrowIfNull(idsServicos);

        var idsServicosSelecionados = idsServicos.Distinct().ToList();
        if (idsServicosSelecionados.Count == 0)
        {
            throw new InvalidOperationException("Selecione ao menos um serviço para o agendamento.");
        }

        var profissionalExiste = context.Profissionals.Any(profissional =>
            profissional.IdPessoa == agendamento.IdProfissional &&
            profissional.IdEstabelecimento == agendamento.IdEstabelecimento &&
            profissional.EstaAtivo == 1);
        if (!profissionalExiste)
        {
            throw new InvalidOperationException(
                "O profissional selecionado não está ativo neste estabelecimento.");
        }

        var servicosDoProfissional = context.Servicoprofissionals
            .AsNoTracking()
            .Where(servicoProfissional =>
                servicoProfissional.IdProfissional == agendamento.IdProfissional &&
                servicoProfissional.IdEstabelecimento == agendamento.IdEstabelecimento &&
                idsServicosSelecionados.Contains(servicoProfissional.IdServico))
            .ToList();

        if (servicosDoProfissional.Count != idsServicosSelecionados.Count)
        {
            throw new InvalidOperationException(
                "Um ou mais serviços selecionados não estão disponíveis para esse profissional.");
        }

        if (servicosDoProfissional.Any(servico =>
                servico.Preco < 0 ||
                servico.Duracao == 0 ||
                servico.PercentualSinal < 0 ||
                servico.PercentualSinal > 100 ||
                (servico.QuandoPagarSinal != QuandoPagarPrimeiroAgendamento &&
                 servico.QuandoPagarSinal != QuandoPagarTodoAgendamento)))
        {
            throw new InvalidOperationException(
                "Há um serviço selecionado com preço, duração ou configuração de sinal inválida.");
        }

        var duracaoTotal = servicosDoProfissional.Aggregate(
            0UL,
            (total, servico) => total + servico.Duracao);
        if (duracaoTotal > uint.MaxValue)
        {
            throw new InvalidOperationException("A duração total dos serviços excede o limite permitido.");
        }

        var dataHoraFimAtendimento =
            agendamento.DataHoraAtendimento.AddMinutes((uint)duracaoTotal);
        var nomeDiaSemana = ObterNomeDiaSemana(agendamento.DataHoraAtendimento.DayOfWeek);
        var estaDentroDoHorarioDeTrabalho = dataHoraFimAtendimento.Date ==
                agendamento.DataHoraAtendimento.Date &&
            context.Horarios.Any(horario =>
                horario.IdProfissional == agendamento.IdProfissional &&
                horario.IdEstabelecimento == agendamento.IdEstabelecimento &&
                horario.DiaSemana == nomeDiaSemana &&
                horario.InicioVigencia.Date <= agendamento.DataHoraAtendimento.Date &&
                (!horario.FimVigencia.HasValue ||
                 horario.FimVigencia.Value.Date >= agendamento.DataHoraAtendimento.Date) &&
                agendamento.DataHoraAtendimento.TimeOfDay >= horario.InicioTurno &&
                dataHoraFimAtendimento.TimeOfDay <= horario.FimTurno);
        if (!estaDentroDoHorarioDeTrabalho)
        {
            throw new InvalidOperationException(
                "O horário selecionado está fora do expediente do profissional.");
        }

        var existeConflito = context.Agendamentos.Any(outroAgendamento =>
            outroAgendamento.IdEstabelecimento == agendamento.IdEstabelecimento &&
            outroAgendamento.IdProfissional == agendamento.IdProfissional &&
            outroAgendamento.StatusAgendamento != StatusAgendamentoCancelado &&
            outroAgendamento.DataHoraAtendimento < dataHoraFimAtendimento &&
            outroAgendamento.DataHoraFimAtendimento > agendamento.DataHoraAtendimento);
        if (existeConflito)
        {
            throw new InvalidOperationException(
                "O horário selecionado não está mais disponível para esse profissional.");
        }

        agendamento.PrecoFinal = servicosDoProfissional.Sum(
            servico => decimal.CreateChecked(servico.Preco));
        agendamento.DuracaoMinutos = (uint)duracaoTotal;
        agendamento.DataHoraFimAtendimento = dataHoraFimAtendimento;
        agendamento.StatusAgendamento = StatusAgendamentoPendente;
        agendamento.StatusPagamento = StatusPagamentoMarcado;
        agendamento.ValorPagoSinal = 0;
        agendamento.Agendamentoservicos = servicosDoProfissional
            .Select(servico => new Agendamentoservico
            {
                IdServico = servico.IdServico,
                PrecoCobrado = servico.Preco
            })
            .ToList();

        context.Agendamentos.Add(agendamento);
        context.SaveChanges();
        return agendamento.Id;
    }

    public void Cancel(uint id, uint idEstabelecimento)
    {
        var agendamento = context.Agendamentos.FirstOrDefault(item =>
            item.Id == id && item.IdEstabelecimento == idEstabelecimento);

        if (agendamento == null)
        {
            return;
        }

        if (agendamento.StatusAgendamento == StatusAgendamentoCancelado ||
            agendamento.StatusAgendamento == StatusAgendamentoConcluido ||
            agendamento.StatusPagamento == StatusPagamentoFinalizado)
        {
            throw new InvalidOperationException(
                "Não é possível cancelar um agendamento já cancelado ou finalizado.");
        }

        agendamento.StatusAgendamento = StatusAgendamentoCancelado;
        agendamento.StatusPagamento = StatusPagamentoCancelado;
        context.SaveChanges();
    }

    public void ConfirmarSinal(uint id, uint idEstabelecimento)
    {
        var agendamento = context.Agendamentos
            .Include(item => item.Agendamentoservicos)
            .FirstOrDefault(item =>
                item.Id == id && item.IdEstabelecimento == idEstabelecimento);

        if (agendamento == null)
        {
            return;
        }

        if (agendamento.StatusAgendamento != StatusAgendamentoPendente ||
            agendamento.StatusPagamento != StatusPagamentoMarcado)
        {
            throw new InvalidOperationException(
                "Não é possível confirmar o sinal de um agendamento que não está pendente.");
        }

        var idsServicos = agendamento.Agendamentoservicos
            .Select(item => item.IdServico)
            .ToList();
        var servicosDoAgendamento = context.Servicoprofissionals
            .AsNoTracking()
            .Where(servico =>
                servico.IdProfissional == agendamento.IdProfissional &&
                servico.IdEstabelecimento == agendamento.IdEstabelecimento &&
                idsServicos.Contains(servico.IdServico))
            .ToList();

        var servicosComSinalDevido = ObterServicosComSinalDevido(agendamento, servicosDoAgendamento);
        var precosCobrados = agendamento.Agendamentoservicos
            .ToDictionary(item => item.IdServico, item => item.PrecoCobrado);
        var valorSinal = servicosComSinalDevido.Sum(servico =>
            precosCobrados[servico.IdServico] * (decimal)servico.PercentualSinal / 100m);

        if (valorSinal <= 0)
        {
            throw new InvalidOperationException(
                "Este agendamento não possui sinal pendente para confirmar.");
        }

        agendamento.ValorPagoSinal = decimal.Round(valorSinal, 2, MidpointRounding.AwayFromZero);
        agendamento.StatusAgendamento = StatusAgendamentoSinalPago;
        context.SaveChanges();
    }

    private static IQueryable<Agendamento> IncluirDadosRelacionados(
        IQueryable<Agendamento> consulta)
    {
        return consulta
            .Include(agendamento => agendamento.IdPessoaClienteNavigation)
            .Include(agendamento => agendamento.Profissional)
                .ThenInclude(profissional => profissional.IdPessoaNavigation)
            .Include(agendamento => agendamento.Agendamentoservicos)
                .ThenInclude(agendamentoServico => agendamentoServico.IdServicoNavigation);
    }

    private static string ObterNomeDiaSemana(DayOfWeek diaSemana)
    {
        return diaSemana switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Segunda-feira",
            DayOfWeek.Tuesday => "Terça-feira",
            DayOfWeek.Wednesday => "Quarta-feira",
            DayOfWeek.Thursday => "Quinta-feira",
            DayOfWeek.Friday => "Sexta-feira",
            DayOfWeek.Saturday => "Sábado",
            _ => throw new ArgumentOutOfRangeException(nameof(diaSemana))
        };
    }

    private List<Servicoprofissional> ObterServicosComSinalDevido(
        Agendamento agendamento,
        List<Servicoprofissional> servicosDoAgendamento)
    {
        var idsServicosSomentePrimeiroAgendamento = servicosDoAgendamento
            .Where(servico => servico.QuandoPagarSinal == QuandoPagarPrimeiroAgendamento)
            .Select(servico => servico.IdServico)
            .ToList();

        var idsServicosJaAgendados = idsServicosSomentePrimeiroAgendamento.Count == 0
            ? new HashSet<uint>()
            : context.Agendamentoservicos
                .Where(agendamentoServico =>
                    idsServicosSomentePrimeiroAgendamento.Contains(agendamentoServico.IdServico) &&
                    agendamentoServico.IdAgendamento != agendamento.Id &&
                    agendamentoServico.IdAgendamentoNavigation.IdPessoaCliente == agendamento.IdPessoaCliente &&
                    agendamentoServico.IdAgendamentoNavigation.IdProfissional == agendamento.IdProfissional &&
                    agendamentoServico.IdAgendamentoNavigation.StatusAgendamento !=
                    StatusAgendamentoCancelado)
                .Select(agendamentoServico => agendamentoServico.IdServico)
                .Distinct()
                .ToHashSet();

        return servicosDoAgendamento
            .Where(servico =>
                servico.QuandoPagarSinal == QuandoPagarTodoAgendamento ||
                (servico.QuandoPagarSinal == QuandoPagarPrimeiroAgendamento &&
                 !idsServicosJaAgendados.Contains(servico.IdServico)))
            .ToList();
    }
}
