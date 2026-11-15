using Core;
using Core.DTO;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Service
{
    public class DashboardService : IDashboardService
    {
        private readonly BemMeCareContext context;

        public DashboardService(BemMeCareContext context)
        {
            this.context = context;
        }

        public DashboardResumoServicosDTO GetResumoServicos(uint idEstabelecimento)
        {
            ValidarEstabelecimento(idEstabelecimento);

            var resumo = new DashboardResumoServicosDTO();

            var servicosAgrupados = (from ags in context.Agendamentoservicos.AsNoTracking()
                                     join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                                     join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                                     where a.IdEstabelecimento == idEstabelecimento
                                        && s.IdEstabelecimento == idEstabelecimento
                                        && a.StatusAgendamento != "CA"
                                     group ags by new { s.Id, s.Nome } into g
                                     select new DashboardItemDTO
                                     {
                                         Label = g.Key.Nome,
                                         Quantidade = g.Count()
                                     })
                                     .OrderByDescending(x => x.Quantidade)
                                     .ThenBy(x => x.Label)
                                     .ToList();

            if (servicosAgrupados.Any())
            {
                var maisUsado = servicosAgrupados.First();
                var menosUsado = servicosAgrupados.Last();

                resumo.ServicoMaisUsadoNome = maisUsado.Label;
                resumo.ServicoMaisUsadoQuantidade = maisUsado.Quantidade;

                resumo.ServicoMenosUsadoNome = menosUsado.Label;
                resumo.ServicoMenosUsadoQuantidade = menosUsado.Quantidade;
            }

            var categoriaMaisUsada = (from ags in context.Agendamentoservicos.AsNoTracking()
                                      join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                                      join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                                      join c in context.Categoria.AsNoTracking() on s.IdCategoria equals c.Id
                                      where a.IdEstabelecimento == idEstabelecimento
                                         && c.IdEstabelecimento == idEstabelecimento
                                         && a.StatusAgendamento != "CA"
                                      group ags by new { c.Id, c.Nome } into g
                                      select new DashboardItemDTO
                                      {
                                          Label = g.Key.Nome,
                                          Quantidade = g.Count()
                                      })
                                      .OrderByDescending(x => x.Quantidade)
                                      .ThenBy(x => x.Label)
                                      .FirstOrDefault();

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
            ValidarEstabelecimento(idEstabelecimento);
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            bool servicoValido = context.Servicos.AsNoTracking()
                .Any(s => s.Id == idServico && s.IdEstabelecimento == idEstabelecimento);
            if (!servicoValido)
                throw new ArgumentException("O serviço informado não pertence ao estabelecimento.");

            var queryBase = from ags in context.Agendamentoservicos.AsNoTracking()
                            join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                            join p in context.Pessoas.AsNoTracking() on a.IdPessoaCliente equals p.Id
                            where a.IdEstabelecimento == idEstabelecimento
                               && ags.IdServico == idServico
                               && a.StatusAgendamento != "CA"
                               && a.DataHoraAtendimento >= inicio
                               && a.DataHoraAtendimento <= fim
                            select p;

            return AgruparDemografico(queryBase, filtroDemografico);
        }

        public IEnumerable<DashboardItemDTO> GetDistribuicaoPorCategoria(
            uint idEstabelecimento,
            uint idCategoria,
            string filtroDemografico,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarEstabelecimento(idEstabelecimento);
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            bool categoriaValida = context.Categoria.AsNoTracking()
                .Any(c => c.Id == idCategoria && c.IdEstabelecimento == idEstabelecimento);
            if (!categoriaValida)
                throw new ArgumentException("A categoria informada não pertence ao estabelecimento.");

            var queryBase = from ags in context.Agendamentoservicos.AsNoTracking()
                            join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                            join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                            join p in context.Pessoas.AsNoTracking() on a.IdPessoaCliente equals p.Id
                            where a.IdEstabelecimento == idEstabelecimento
                               && s.IdEstabelecimento == idEstabelecimento
                               && s.IdCategoria == idCategoria
                               && a.StatusAgendamento != "CA"
                               && a.DataHoraAtendimento >= inicio
                               && a.DataHoraAtendimento <= fim
                            select p;

            return AgruparDemografico(queryBase, filtroDemografico);
        }

        public DashboardResumoFaturamentoDTO GetResumoFaturamento(uint idEstabelecimento, DateTime? dataReferencia = null)
        {
            ValidarEstabelecimento(idEstabelecimento);

            var referencia = dataReferencia ?? DateTime.Now;
            var inicioMesAtual = new DateTime(referencia.Year, referencia.Month, 1);
            var fimMesAtual = inicioMesAtual.AddMonths(1).AddTicks(-1);

            var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
            var fimMesAnterior = inicioMesAtual.AddTicks(-1);

            var fimSemanaAtual = referencia.Date.AddDays(1).AddTicks(-1);
            var inicioSemanaAtual = referencia.Date.AddDays(-6);

            var fimSemanaAnterior = inicioSemanaAtual.AddTicks(-1);
            var inicioSemanaAnterior = inicioSemanaAtual.AddDays(-7);

            var agendamentosValidos = context.Agendamentos.AsNoTracking()
                .Where(a => a.IdEstabelecimento == idEstabelecimento
                         && a.StatusAgendamento != "CA"
                         && a.StatusPagamento != "CA");

            decimal faturamentoMesAtual = agendamentosValidos
                .Where(a => a.DataHoraAtendimento >= inicioMesAtual && a.DataHoraAtendimento <= fimMesAtual)
                .Sum(a => (decimal?)a.PrecoFinal) ?? 0m;

            decimal faturamentoMesAnterior = agendamentosValidos
                .Where(a => a.DataHoraAtendimento >= inicioMesAnterior && a.DataHoraAtendimento <= fimMesAnterior)
                .Sum(a => (decimal?)a.PrecoFinal) ?? 0m;

            decimal faturamentoSemanal = agendamentosValidos
                .Where(a => a.DataHoraAtendimento >= inicioSemanaAtual && a.DataHoraAtendimento <= fimSemanaAtual)
                .Sum(a => (decimal?)a.PrecoFinal) ?? 0m;

            decimal faturamentoSemanaAnterior = agendamentosValidos
                .Where(a => a.DataHoraAtendimento >= inicioSemanaAnterior && a.DataHoraAtendimento <= fimSemanaAnterior)
                .Sum(a => (decimal?)a.PrecoFinal) ?? 0m;

            var resumo = new DashboardResumoFaturamentoDTO
            {
                FaturamentoMesAtual = faturamentoMesAtual,
                PercentualVariacaoMensal = CalcularVariacaoPercentual(faturamentoMesAtual, faturamentoMesAnterior),
                FaturamentoSemanal = faturamentoSemanal,
                PercentualVariacaoSemanal = CalcularVariacaoPercentual(faturamentoSemanal, faturamentoSemanaAnterior)
            };

            var servicosFaturamento = (from ags in context.Agendamentoservicos.AsNoTracking()
                                       join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                                       join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                                       where a.IdEstabelecimento == idEstabelecimento
                                          && s.IdEstabelecimento == idEstabelecimento
                                          && a.StatusAgendamento != "CA"
                                          && a.StatusPagamento != "CA"
                                       group ags by new { s.Id, s.Nome } into g
                                       select new DashboardItemDTO
                                       {
                                           Label = g.Key.Nome,
                                           Valor = g.Sum(x => x.PrecoCobrado)
                                       })
                                       .OrderByDescending(x => x.Valor)
                                       .ThenBy(x => x.Label)
                                       .ToList();

            if (servicosFaturamento.Any())
            {
                var maisFatura = servicosFaturamento.First();
                var menosFatura = servicosFaturamento.Last();

                resumo.ServicoMaisFaturaNome = maisFatura.Label;
                resumo.ServicoMaisFaturaValor = maisFatura.Valor;

                resumo.ServicoMenosFaturaNome = menosFatura.Label;
                resumo.ServicoMenosFaturaValor = menosFatura.Valor;
            }

            var inicio12Meses = inicioMesAtual.AddMonths(-11);
            var dadosPorMes = agendamentosValidos
                .Where(a => a.DataHoraAtendimento >= inicio12Meses && a.DataHoraAtendimento <= fimMesAtual)
                .GroupBy(a => new { a.DataHoraAtendimento.Year, a.DataHoraAtendimento.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.PrecoFinal)
                })
                .ToList();

            var cultura = new CultureInfo("pt-BR");
            var lista12Meses = new List<DashboardItemDTO>();
            for (int i = 0; i < 12; i++)
            {
                var mesRef = inicio12Meses.AddMonths(i);
                var encontrado = dadosPorMes.FirstOrDefault(d => d.Year == mesRef.Year && d.Month == mesRef.Month);
                string nomeMes = cultura.DateTimeFormat.GetAbbreviatedMonthName(mesRef.Month).TrimEnd('.');
                lista12Meses.Add(new DashboardItemDTO
                {
                    Label = $"{nomeMes}/{mesRef:yy}",
                    Valor = encontrado?.Total ?? 0m
                });
            }

            resumo.FaturamentoUltimos12Meses = lista12Meses;
            return resumo;
        }

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorProfissional(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarEstabelecimento(idEstabelecimento);
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            return (from a in context.Agendamentos.AsNoTracking()
                    join p in context.Pessoas.AsNoTracking() on a.IdProfissional equals p.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento != "CA"
                       && a.StatusPagamento != "CA"
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

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorServico(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarEstabelecimento(idEstabelecimento);
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            return (from ags in context.Agendamentoservicos.AsNoTracking()
                    join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                    join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && s.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento != "CA"
                       && a.StatusPagamento != "CA"
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

        public IEnumerable<DashboardItemDTO> GetFaturamentoPorCategoria(
            uint idEstabelecimento,
            DateTime dataInicio,
            DateTime dataFim)
        {
            ValidarEstabelecimento(idEstabelecimento);
            ValidarPeriodo(dataInicio, dataFim, out var inicio, out var fim);

            return (from ags in context.Agendamentoservicos.AsNoTracking()
                    join a in context.Agendamentos.AsNoTracking() on ags.IdAgendamento equals a.Id
                    join s in context.Servicos.AsNoTracking() on ags.IdServico equals s.Id
                    join c in context.Categoria.AsNoTracking() on s.IdCategoria equals c.Id
                    where a.IdEstabelecimento == idEstabelecimento
                       && c.IdEstabelecimento == idEstabelecimento
                       && a.StatusAgendamento != "CA"
                       && a.StatusPagamento != "CA"
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

        private static IEnumerable<DashboardItemDTO> AgruparDemografico(IQueryable<Pessoa> queryBase, string filtroDemografico)
        {
            if (string.IsNullOrWhiteSpace(filtroDemografico))
                throw new ArgumentException("O filtro demográfico é obrigatório.");

            string chave = filtroDemografico.Trim().ToUpperInvariant();

            List<DashboardItemDTO> agrupado = chave switch
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

        private static void ValidarEstabelecimento(uint idEstabelecimento)
        {
            if (idEstabelecimento == 0)
                throw new ArgumentException("O estabelecimento informado é inválido.");
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