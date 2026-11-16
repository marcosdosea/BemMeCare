using Core;
using Core.DTO;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Service
{
    public class DashboardService : IDashboardService
    {
        private const string StatusAgendamentoCompleto = "CO";
        private readonly BemMeCareContext context;

        public DashboardService(BemMeCareContext context)
        {
            this.context = context;
        }

        public DashboardResumoServicosDTO GetResumoServicos(uint idEstabelecimento)
        {
            var resumo = new DashboardResumoServicosDTO();

            var servicoMaisUsado = ConsultarServicoMaisUsado(idEstabelecimento);
            if (servicoMaisUsado != null)
            {
                resumo.ServicoMaisUsadoNome = servicoMaisUsado.Label;
                resumo.ServicoMaisUsadoQuantidade = servicoMaisUsado.Quantidade;
            }

            var servicoMenosUsado = ConsultarServicoMenosUsado(idEstabelecimento);
            if (servicoMenosUsado != null)
            {
                resumo.ServicoMenosUsadoNome = servicoMenosUsado.Label;
                resumo.ServicoMenosUsadoQuantidade = servicoMenosUsado.Quantidade;
            }

            var categoriaMaisUsada = ConsultarCategoriaMaisUsada(idEstabelecimento);
            if (categoriaMaisUsada != null)
            {
                resumo.CategoriaMaisUsadaNome = categoriaMaisUsada.Label;
                resumo.CategoriaMaisUsadaQuantidade = categoriaMaisUsada.Quantidade;
            }

            return resumo;
        }

        public IEnumerable<DashboardItemDTO> GetDistribuicaoPorServico(
            uint idEstabelecimento,
            uint idServico,
            string filtroDemografico,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            var queryClientes = ConsultarClientesPorServicoNoPeriodo(idEstabelecimento, idServico, inicio, fim);
            return CalcularDistribuicaoDemografica(queryClientes, filtroDemografico);
        }

        public IEnumerable<DashboardItemDTO> GetDistribuicaoPorCategoria(
            uint idEstabelecimento,
            uint idCategoria,
            string filtroDemografico,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            var queryClientes = ConsultarClientesPorCategoriaNoPeriodo(idEstabelecimento, idCategoria, inicio, fim);
            return CalcularDistribuicaoDemografica(queryClientes, filtroDemografico);
        }

        public DashboardResumoFaturamentoDTO GetResumoFaturamento(uint idEstabelecimento, DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.Now;

            var inicioMesAtual = new DateTime(referencia.Year, referencia.Month, 1);
            var fimMesAtual = inicioMesAtual.AddMonths(1).AddTicks(-1);
            var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
            var fimMesAnterior = inicioMesAtual.AddTicks(-1);

            var fimSemanaAtual = referencia.Date.AddDays(1).AddTicks(-1);
            var inicioSemanaAtual = referencia.Date.AddDays(-6);
            var fimSemanaAnterior = inicioSemanaAtual.AddTicks(-1);
            var inicioSemanaAnterior = inicioSemanaAtual.AddDays(-7);

            decimal faturamentoMesAtual = ConsultarSomaFaturamentoNoPeriodo(idEstabelecimento, inicioMesAtual, fimMesAtual);
            decimal faturamentoMesAnterior = ConsultarSomaFaturamentoNoPeriodo(idEstabelecimento, inicioMesAnterior, fimMesAnterior);
            decimal faturamentoSemanal = ConsultarSomaFaturamentoNoPeriodo(idEstabelecimento, inicioSemanaAtual, fimSemanaAtual);
            decimal faturamentoSemanaAnterior = ConsultarSomaFaturamentoNoPeriodo(idEstabelecimento, inicioSemanaAnterior, fimSemanaAnterior);

            var resumo = new DashboardResumoFaturamentoDTO
            {
                FaturamentoMesAtual = faturamentoMesAtual,
                PercentualVariacaoMensal = CalcularVariacaoPercentual(faturamentoMesAtual, faturamentoMesAnterior),
                FaturamentoSemanal = faturamentoSemanal,
                PercentualVariacaoSemanal = CalcularVariacaoPercentual(faturamentoSemanal, faturamentoSemanaAnterior),
                FaturamentoUltimos12Meses = MontarSerieUltimos12Meses(idEstabelecimento, inicioMesAtual, fimMesAtual)
            };

            var maisFatura = ConsultarServicoQueMaisFatura(idEstabelecimento);
            if (maisFatura != null)
            {
                resumo.ServicoMaisFaturaNome = maisFatura.Label;
                resumo.ServicoMaisFaturaValor = maisFatura.Valor;
            }

            var menosFatura = ConsultarServicoQueMenosFatura(idEstabelecimento);
            if (menosFatura != null)
            {
                resumo.ServicoMenosFaturaNome = menosFatura.Label;
                resumo.ServicoMenosFaturaValor = menosFatura.Valor;
            }

            return resumo;
        }

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorProfissional(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);
            return ConsultarFaturamentoPorProfissionalNoPeriodo(idEstabelecimento, inicio, fim);
        }

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorServico(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);
            return ConsultarFaturamentoPorServicoNoPeriodo(idEstabelecimento, inicio, fim);
        }

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorCategoria(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);
            return ConsultarFaturamentoPorCategoriaNoPeriodo(idEstabelecimento, inicio, fim);
        }

        private IQueryable<DashboardItemDTO> ConsultarBaseServicosPorQuantidade(uint idEstabelecimento)
        {
            return from ags in context.Agendamentoservicos.AsNoTracking()
                   join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                   join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                   where a.IdEstabelecimento == idEstabelecimento
                      && s.IdEstabelecimento == idEstabelecimento
                      && a.StatusAgendamento == StatusAgendamentoCompleto
                   group ags by new { s.Id, s.Nome } into g
                   select new DashboardItemDTO
                   {
                       Label = g.Key.Nome,
                       Quantidade = g.Count()
                   };
        }

        private DashboardItemDTO? ConsultarServicoMaisUsado(uint idEstabelecimento)
        {
            return ConsultarBaseServicosPorQuantidade(idEstabelecimento)
                .OrderByDescending(x => x.Quantidade)
                .ThenBy(x => x.Label)
                .FirstOrDefault();
        }

        private DashboardItemDTO? ConsultarServicoMenosUsado(uint idEstabelecimento)
        {
            return ConsultarBaseServicosPorQuantidade(idEstabelecimento)
                .OrderBy(x => x.Quantidade)
                .ThenBy(x => x.Label)
                .FirstOrDefault();
        }

        private DashboardItemDTO? ConsultarCategoriaMaisUsada(uint idEstabelecimento)
        {
            return (from ags in context.Agendamentoservicos.AsNoTracking()
                    join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                    join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                    join c in context.Categoria.AsNoTracking() on s.IdCategoria equals c.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && c.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento == StatusAgendamentoCompleto
                    group ags by new { c.Id, c.Nome } into g
                    select new DashboardItemDTO
                    {
                        Label = g.Key.Nome,
                        Quantidade = g.Count()
                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ThenBy(x => x.Label)
                    .FirstOrDefault();
        }

        private IQueryable<Pessoa> ConsultarClientesPorServicoNoPeriodo(
            uint idEstabelecimento,
            uint idServico,
            DateTime inicio,
            DateTime fim)
        {
            return from ags in context.Agendamentoservicos.AsNoTracking()
                   join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                   join p in context.Pessoas.AsNoTracking() on a.IdPessoaCliente equals p.Id
                   where a.IdEstabelecimento == idEstabelecimento
                      && ags.IdServico == idServico
                      && a.StatusAgendamento == StatusAgendamentoCompleto
                      && a.DataHoraAtendimento >= inicio
                      && a.DataHoraAtendimento <= fim
                   select p;
        }

        private IQueryable<Pessoa> ConsultarClientesPorCategoriaNoPeriodo(
            uint idEstabelecimento,
            uint idCategoria,
            DateTime inicio,
            DateTime fim)
        {
            return from ags in context.Agendamentoservicos.AsNoTracking()
                   join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                   join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                   join p in context.Pessoas.AsNoTracking() on a.IdPessoaCliente equals p.Id
                   where a.IdEstabelecimento == idEstabelecimento
                      && s.IdEstabelecimento == idEstabelecimento
                      && s.IdCategoria == idCategoria
                      && a.StatusAgendamento == StatusAgendamentoCompleto
                      && a.DataHoraAtendimento >= inicio
                      && a.DataHoraAtendimento <= fim
                   select p;
        }

        private static List<DashboardItemDTO> ConsultarAgrupamentoDemografico(IQueryable<Pessoa> queryBase, string chaveDemografica)
        {
            return chaveDemografica switch
            {
                "GENERO" or "GÊNERO" => queryBase
                    .GroupBy(p => p.Genero)
                    .Select(g => new DashboardItemDTO { Label = g.Key, Quantidade = g.Count() })
                    .ToList(),
                "SEXUALIDADE" => queryBase
                    .GroupBy(p => p.Sexualidade)
                    .Select(g => new DashboardItemDTO { Label = g.Key, Quantidade = g.Count() })
                    .ToList(),
                "COR" => queryBase
                    .GroupBy(p => p.Cor)
                    .Select(g => new DashboardItemDTO { Label = g.Key, Quantidade = g.Count() })
                    .ToList(),
                _ => throw new ArgumentException("Filtro demográfico inválido. Use Gênero, Sexualidade ou Cor.")
            };
        }

        private decimal ConsultarSomaFaturamentoNoPeriodo(uint idEstabelecimento, DateTime inicio, DateTime fim)
        {
            return context.Agendamentos.AsNoTracking()
                .Where(a => a.IdEstabelecimento == idEstabelecimento
                         && a.StatusAgendamento == StatusAgendamentoCompleto
                         && a.DataHoraAtendimento >= inicio
                         && a.DataHoraAtendimento <= fim)
                .Sum(a => (decimal?)a.PrecoFinal) ?? 0m;
        }

        private IQueryable<DashboardItemDTO> ConsultarBaseServicosPorFaturamento(uint idEstabelecimento)
        {
            return from ags in context.Agendamentoservicos.AsNoTracking()
                   join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                   join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                   where a.IdEstabelecimento == idEstabelecimento
                      && s.IdEstabelecimento == idEstabelecimento
                      && a.StatusAgendamento == StatusAgendamentoCompleto
                   group ags by new { s.Id, s.Nome } into g
                   select new DashboardItemDTO
                   {
                       Label = g.Key.Nome,
                       Valor = g.Sum(x => x.PrecoCobrado)
                   };
        }

        private DashboardItemDTO? ConsultarServicoQueMaisFatura(uint idEstabelecimento)
        {
            return ConsultarBaseServicosPorFaturamento(idEstabelecimento)
                .OrderByDescending(x => x.Valor)
                .ThenBy(x => x.Label)
                .FirstOrDefault();
        }

        private DashboardItemDTO? ConsultarServicoQueMenosFatura(uint idEstabelecimento)
        {
            return ConsultarBaseServicosPorFaturamento(idEstabelecimento)
                .OrderBy(x => x.Valor)
                .ThenBy(x => x.Label)
                .FirstOrDefault();
        }

        private Dictionary<(int Ano, int Mes), decimal> ConsultarFaturamentoAgrupadoPorMes(
            uint idEstabelecimento,
            DateTime inicio,
            DateTime fim)
        {
            return context.Agendamentos.AsNoTracking()
                .Where(a => a.IdEstabelecimento == idEstabelecimento
                         && a.StatusAgendamento == StatusAgendamentoCompleto
                         && a.DataHoraAtendimento >= inicio
                         && a.DataHoraAtendimento <= fim)
                .GroupBy(a => new { a.DataHoraAtendimento.Year, a.DataHoraAtendimento.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.PrecoFinal)
                })
                .ToList()
                .ToDictionary(x => (x.Year, x.Month), x => x.Total);
        }

        private List<DashboardItemDTO> ConsultarFaturamentoPorProfissionalNoPeriodo(
            uint idEstabelecimento,
            DateTime inicio,
            DateTime fim)
        {
            return (from a in context.Agendamentos.AsNoTracking()
                    join p in context.Pessoas.AsNoTracking() on a.IdProfissional equals p.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento == StatusAgendamentoCompleto
                       && a.DataHoraAtendimento >= inicio
                       && a.DataHoraAtendimento <= fim
                    group a by new { a.IdProfissional, p.Nome } into g
                    select new DashboardItemDTO
                    {
                        Label = g.Key.Nome,
                        Quantidade = g.Count(),
                        Valor = g.Sum(x => x.PrecoFinal)
                    })
                    .OrderByDescending(x => x.Valor)
                    .ThenBy(x => x.Label)
                    .ToList();
        }

        private List<DashboardItemDTO> ConsultarFaturamentoPorServicoNoPeriodo(
            uint idEstabelecimento,
            DateTime inicio,
            DateTime fim)
        {
            return (from ags in context.Agendamentoservicos.AsNoTracking()
                    join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                    join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && s.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento == StatusAgendamentoCompleto
                       && a.DataHoraAtendimento >= inicio
                       && a.DataHoraAtendimento <= fim
                    group ags by new { s.Id, s.Nome } into g
                    select new DashboardItemDTO
                    {
                        Label = g.Key.Nome,
                        Quantidade = g.Count(),
                        Valor = g.Sum(x => x.PrecoCobrado)
                    })
                    .OrderByDescending(x => x.Valor)
                    .ThenBy(x => x.Label)
                    .ToList();
        }

        private List<DashboardItemDTO> ConsultarFaturamentoPorCategoriaNoPeriodo(
            uint idEstabelecimento,
            DateTime inicio,
            DateTime fim)
        {
            return (from ags in context.Agendamentoservicos.AsNoTracking()
                    join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                    join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                    join c in context.Categoria.AsNoTracking() on s.IdCategoria equals c.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && c.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento == StatusAgendamentoCompleto
                       && a.DataHoraAtendimento >= inicio
                       && a.DataHoraAtendimento <= fim
                    group ags by new { c.Id, c.Nome } into g
                    select new DashboardItemDTO
                    {
                        Label = g.Key.Nome,
                        Quantidade = g.Count(),
                        Valor = g.Sum(x => x.PrecoCobrado)
                    })
                    .OrderByDescending(x => x.Valor)
                    .ThenBy(x => x.Label)
                    .ToList();
        }

        private IEnumerable<DashboardItemDTO> MontarSerieUltimos12Meses(
            uint idEstabelecimento,
            DateTime inicioMesAtual,
            DateTime fimMesAtual)
        {
            var inicio12Meses = inicioMesAtual.AddMonths(-11);
            var totaisPorMes = ConsultarFaturamentoAgrupadoPorMes(idEstabelecimento, inicio12Meses, fimMesAtual);

            var cultura = new CultureInfo("pt-BR");
            var lista12Meses = new List<DashboardItemDTO>();

            for (int i = 0; i < 12; i++)
            {
                var mesRef = inicio12Meses.AddMonths(i);
                totaisPorMes.TryGetValue((mesRef.Year, mesRef.Month), out decimal valorMes);
                string nomeMes = cultura.DateTimeFormat.GetAbbreviatedMonthName(mesRef.Month).TrimEnd('.');

                lista12Meses.Add(new DashboardItemDTO
                {
                    Label = $"{nomeMes}/{mesRef:yy}",
                    Valor = valorMes
                });
            }

            return lista12Meses;
        }

        private static IEnumerable<DashboardItemDTO> CalcularDistribuicaoDemografica(
            IQueryable<Pessoa> queryBase,
            string filtroDemografico)
        {
            if (string.IsNullOrWhiteSpace(filtroDemografico))
                throw new ArgumentException("O filtro demográfico é obrigatório.");

            string chave = filtroDemografico.Trim().ToUpperInvariant();
            var agrupado = ConsultarAgrupamentoDemografico(queryBase, chave);

            int total = agrupado.Sum(x => x.Quantidade);
            if (total == 0)
                return Enumerable.Empty<DashboardItemDTO>();

            foreach (var item in agrupado)
            {
                if (string.IsNullOrWhiteSpace(item.Label))
                    item.Label = "Não informado";

                item.Percentual = Math.Round((decimal)item.Quantidade / total * 100m, 2);
            }

            return agrupado.OrderByDescending(x => x.Quantidade).ToList();
        }

        private static void ValidarPeriodo(DateTime dataInicio, DateTime dataFim, out DateTime inicio, out DateTime fim)
        {
            if (dataInicio.Date > dataFim.Date)
                throw new ArgumentException("A data de início não pode ser maior que a data de fim.");

            inicio = dataInicio.Date;
            fim = dataFim.Date.AddDays(1).AddTicks(-1);
        }

        private static decimal CalcularVariacaoPercentual(decimal valorAtual, decimal valorAnterior)
        {
            if (valorAnterior == 0m)
                return valorAtual > 0m ? 100m : 0m;

            return Math.Round((valorAtual - valorAnterior) / valorAnterior * 100m, 2);
        }
    }
}