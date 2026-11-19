using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class AgendamentoDeClientesMapper
{
    private static readonly TypeAdapterConfig Config = CriarConfiguracao();

    public static AgendamentoDeClientesViewModel ToViewModel(this Agendamento agendamento)
    {
        return agendamento.Adapt<AgendamentoDeClientesViewModel>(Config);
    }

    public static IEnumerable<AgendamentoDeClientesViewModel> ToViewModel(
        this IEnumerable<Agendamento> agendamentos)
    {
        return agendamentos.Select(agendamento => agendamento.ToViewModel());
    }

    public static Agendamento ToEntity(this AgendamentoDeClientesViewModel viewModel)
    {
        if (!viewModel.DataHoraAtendimento.HasValue)
        {
            throw new InvalidOperationException("A data e o horário do atendimento são obrigatórios.");
        }

        return viewModel.Adapt<Agendamento>(Config);
    }

    private static TypeAdapterConfig CriarConfiguracao()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<Agendamento, AgendamentoDeClientesViewModel>()
            .Map(destino => destino.NomeCliente, origem => origem.IdPessoaClienteNavigation.Nome)
            .Map(destino => destino.TelefoneCliente, origem => origem.IdPessoaClienteNavigation.Telefone)
            .Map(destino => destino.NomeProfissional, origem => origem.Profissional.IdPessoaNavigation.Nome)
            .Map(
                destino => destino.NomesServicos,
                origem => string.Join(
                    ", ",
                    origem.Agendamentoservicos.Select(agendamentoServico =>
                        agendamentoServico.IdServicoNavigation.Nome)));

        config.NewConfig<AgendamentoDeClientesViewModel, Agendamento>()
            .Ignore(destino => destino.Id)
            .Map(destino => destino.DataHoraAtendimento, origem => origem.DataHoraAtendimento!.Value)
            .Map(destino => destino.Observacoes, origem => origem.Observacoes)
            .Map(destino => destino.IdProfissional, origem => origem.IdProfissional)
            .Ignore(destino => destino.PrecoFinal)
            .Ignore(destino => destino.DuracaoMinutos)
            .Ignore(destino => destino.StatusAgendamento)
            .Ignore(destino => destino.StatusPagamento)
            .Ignore(destino => destino.DataHoraCriacao)
            .Ignore(destino => destino.ValorPagoSinal)
            .Ignore(destino => destino.DataHoraFimAtendimento)
            .Ignore(destino => destino.IdPessoaCliente)
            .Ignore(destino => destino.IdEstabelecimento)
            .Ignore(destino => destino.IdPessoaMarcou)
            .Ignore(destino => destino.Agendamentoservicos)
            .Ignore(destino => destino.IdPessoaClienteNavigation)
            .Ignore(destino => destino.IdPessoaMarcouNavigation)
            .Ignore(destino => destino.Profissional);

        return config;
    }
}
