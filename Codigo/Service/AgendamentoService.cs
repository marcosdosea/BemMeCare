using Core;
using Core.DTO;
using Core.Service;
using Microsoft.EntityFrameworkCore;


namespace Service
{
    public class AgendamentoService : IAgendamentoService
    {
        private const string StatusAgendamentoNormal = "MA";
        private const string StatusPagamentoInicial = "PE";
        private const string StatusAgendamentoCancelado = "CA";
        private const string StatusAgendamentoConcluido = "CO";
        private const string StatusPagamentoCancelado = "CA";
        private const string StatusPagamentoFinalizado = "FI";
        private readonly BemMeCareContext context;

        public AgendamentoService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cria um novo agendamento.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <param name="idPessoa">Id do cliente.</param>
        /// <returns>Id do agendamento criado.</returns>
        public uint Create(SelecaoAgendamentoDTO selecao, uint idEstabelecimento, uint idPessoa)
        {
            if (selecao.DataDoAgendamento is null || selecao.HorarioAgendamento is null)
            {
                throw new Exception();
            }

            if (!ValidateProfissionalServicosAgendamento(selecao, idEstabelecimento))
            {
                throw new Exception();
            }

            var data = selecao.DataDoAgendamento.Value;
            var horario = selecao.HorarioAgendamento.Value;

            var resumo = GetResumoAgendamento(selecao, idEstabelecimento);

            var inicio = data.ToDateTime(horario);

            var agendamento = new Agendamento
            {
                IdEstabelecimento = idEstabelecimento,
                IdProfissional = selecao.IdProfissional,
                IdPessoaCliente = idPessoa,
                IdPessoaMarcou = idPessoa,
                DataHoraAtendimento = inicio,
                DataHoraFimAtendimento = inicio.AddMinutes(resumo.DuracaoTotalEmMinutos),
                DuracaoMinutos = resumo.DuracaoTotalEmMinutos,
                PrecoFinal = resumo.PrecoTotal,
                ValorPagoSinal = 0,
                StatusAgendamento = StatusAgendamentoNormal,
                StatusPagamento = StatusPagamentoInicial,
                DataHoraCriacao = DateTime.Now,
                Observacoes = null,
                Agendamentoservicos = resumo.Servicos
                    .Select(s => new Agendamentoservico
                    {
                        IdServico = s.Id,
                        PrecoCobrado = s.Preco
                    })
                    .ToList()
            };

            context.Agendamentos.Add(agendamento);
            context.SaveChanges();

            return agendamento.Id;
        }

        /// <summary>
        /// Cancela um agendamento.
        /// </summary>
        /// <param name="id">Id do agendamento.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento do agendamento.</param>
        /// <returns>Verdadeiro se o agendamento for cancelado. Falso, caso contrário.</returns>
        public bool Cancel(uint id, uint idEstabelecimento)
        {
            var agendamento = context.Agendamentos.FirstOrDefault(a => a.Id == id && a.IdEstabelecimento == idEstabelecimento);

            if (agendamento == null)
            {
                return false;
            }

            if (agendamento.StatusAgendamento == StatusAgendamentoCancelado
                || agendamento.StatusAgendamento == StatusAgendamentoConcluido
                || agendamento.StatusPagamento == StatusPagamentoFinalizado)
            {
                return false;
            }

            agendamento.StatusAgendamento = StatusAgendamentoCancelado;
            agendamento.StatusPagamento = StatusPagamentoCancelado;
            context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Obtém todos os agendamentos de uma pessoa em um estabelecimento.
        /// </summary>
        /// <param name="idPessoa">Id do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <returns>Lista com todos os agendamentos do cliente.</returns>
        public IEnumerable<AgendamentoDTO> GetAll(uint idPessoa, uint idEstabelecimento)
        {
            return context.Agendamentos
                .AsNoTracking()
                .Where(a => a.IdPessoaCliente == idPessoa && a.IdEstabelecimento == idEstabelecimento)
                .Select(a => new AgendamentoDTO
                {
                    NomeProfissional = a.Profissional.IdPessoaNavigation.Nome,
                    HorarioAtendimento = a.DataHoraAtendimento,
                    PrecoTotal = a.PrecoFinal,
                    DuracaoTotalEmMinutos = a.DuracaoMinutos,
                    Servicos = a.Agendamentoservicos
                        .Select(ass => ass.IdServicoNavigation.Nome)
                        .ToList()
                }).ToList();
        }

        /// <summary>
        /// Verifica se os serviços selecionados são válidos para o profissional e para o estabelecimento,
        /// além de verificar regras para impedir que nenhum serviço ou serviços duplicados sejam
        /// selecionados.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <returns>Verdadeiro se a lista de serviços selecionados for válida. Falso, caso contrário..</returns>
        public bool ValidateProfissionalServicosAgendamento(SelecaoAgendamentoDTO selecao, uint idEstabelecimento)
        {
            if (ListaServicosVazia(selecao)) 
            {
                return false;
            }

            if (ListaTemServicosDuplicados(selecao))
            {
                return false;
            }

            return ServicosPertencemProfissionalEstabelecimento(selecao, idEstabelecimento);
        }

        /// <summary>
        /// Obtém os horários de início disponíveis para o agendamento, encadeando blocos
        /// a partir do início de cada turno até o fim de cada agendamento existente.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="turnos">Turnos de um profissional.</param>
        /// <returns>Lista com os horários disponíveis do profissional para realizar o agendamento.</returns>
        public IEnumerable<TimeOnly> GetHorariosDisponiveisProfissionalAgendamento(SelecaoAgendamentoDTO selecao, IEnumerable<Horario> turnos, uint idEstabelecimento)
        {
            if (selecao.DataDoAgendamento is null)
            {
                return [];
            }

            var data = selecao.DataDoAgendamento.Value;
            var agora = DateTime.Now;
            var hoje = DateOnly.FromDateTime(agora);

            if (data < hoje)
            {
                return [];
            }
            var duracao = TimeSpan.FromMinutes(GetDuracaoTotalEmMinutos(selecao, idEstabelecimento));

            if (duracao <= TimeSpan.Zero)
            {
                return [];
            }

            var inicioDoDia = data.ToDateTime(TimeOnly.MinValue);
            var fimDoDia = inicioDoDia.AddDays(1);

            var ocupados = context.Agendamentos
                .AsNoTracking()
                .Where(a => a.IdProfissional == selecao.IdProfissional
                         && a.IdEstabelecimento == idEstabelecimento
                         && a.StatusAgendamento != "CA"
                         && a.DataHoraAtendimento < fimDoDia
                         && a.DataHoraFimAtendimento > inicioDoDia)
                .Select(a => new { a.DataHoraAtendimento, a.DataHoraFimAtendimento })
                .ToList()
                .Select(a => (Inicio: a.DataHoraAtendimento, Fim: a.DataHoraFimAtendimento))
                .ToList();

            agora = DateTime.Now;
            DateTime? limiteMinimo = null;
            if (data == DateOnly.FromDateTime(agora))
            {
                limiteMinimo = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, 0)
                    .AddMinutes(1);
            }

            var horarios = new List<TimeOnly>();

            foreach (var turno in turnos)
            {
                var inicioTurno = inicioDoDia + turno.InicioTurno;
                var fimTurno = inicioDoDia + turno.FimTurno;

                foreach (var (livreInicio, livreFim) in GetIntervalosLivres(inicioTurno, fimTurno, ocupados))
                {
                    var inicio = livreInicio;
                    if (limiteMinimo is not null && inicio < limiteMinimo)
                    {
                        inicio = limiteMinimo.Value;
                    }

                    for (var t = inicio; t + duracao <= livreFim; t += duracao)
                    {
                        horarios.Add(TimeOnly.FromDateTime(t));
                    }
                }
            }

            return horarios.Distinct().OrderBy(h => h).ToList();
        }

        /// <summary>
        /// Monta o resumo do agendamento com nomes, preços e duração recalculados.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <returns>DTO com as informações do resumo do agendamento prestes a ser criado..</returns>
        public AgendamentoResumoDTO GetResumoAgendamento(SelecaoAgendamentoDTO selecao, uint idEstabelecimento)
        {
            if (selecao.HorarioAgendamento is null)
            {
                throw new InvalidOperationException("O horário do agendamento é obrigatório para o resumo.");
            }

            var ids = selecao.IdServicosSelecionados.ToList();

            var profissional = context.Profissionals
                .AsNoTracking()
                .Where(p => p.EstaAtivo == 1
                         && p.IdPessoa == selecao.IdProfissional
                         && p.IdEstabelecimento == idEstabelecimento)
                .Select(p => new
                {
                    Nome = p.IdPessoaNavigation.Nome,
                    Servicos = p.Servicoprofissionals
                        .Where(sp => sp.IdEstabelecimento == idEstabelecimento
                                  && ids.Contains(sp.IdServico))
                        .Select(sp => new ServicoProfissionalDTO
                        {
                            Id = sp.IdServico,
                            Nome = sp.IdServicoNavigation.Nome,
                            DuracaoEmMinutos = sp.Duracao,
                            Preco = sp.Preco
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (profissional is null || profissional.Servicos.Count != ids.Count)
            {
                throw new InvalidOperationException("A seleção de serviços não é válida para este profissional.");
            }

            var servicos = profissional.Servicos
                .OrderBy(s => s.Nome)
                .ToList();

            return new AgendamentoResumoDTO
            {
                NomeProfissional = profissional.Nome,
                Servicos = servicos,
                PrecoTotal = servicos.Sum(s => s.Preco),
                DuracaoTotalEmMinutos = (uint)servicos.Sum(s => (long)s.DuracaoEmMinutos),
                HorarioAtendimento = selecao.HorarioAgendamento.Value
            };
        }

        /// <summary>
        /// Verifica se a lista de serviços selecionados para agendamento está vazia.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <returns>Verdadeiro se a lista está vazia. Falso, caso contrário.</returns>
        private bool ListaServicosVazia(SelecaoAgendamentoDTO selecao)
        {
            return selecao.IdServicosSelecionados.Count() == 0;
        }

        /// <summary>
        /// Verifica se a lista dos serviços selecionados possui serviços duplicados.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <returns>Verdadeiro se possui serviços duplicados na lista. Falso, caso contrário.</returns>
        private bool ListaTemServicosDuplicados(SelecaoAgendamentoDTO selecao)
        {
            var conjunto = new HashSet<uint>();

            return selecao.IdServicosSelecionados.Any(id => !conjunto.Add(id));
        }

        /// <summary>
        /// Verifica se os serviços selecionados realmente pertencem a um profissional.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <returns>Verdadeiro se os serviços selecionados pertencem ao profissional. Falso, caso contrário..</returns>
        private bool ServicosPertencemProfissionalEstabelecimento(SelecaoAgendamentoDTO selecao, uint idEstabelecimento)
        {
            var servicosEscolhidos = selecao.IdServicosSelecionados;
            var servicosProfissional = context.Profissionals
                .AsNoTracking()
                .Where(p => p.EstaAtivo == 1
                        && p.IdPessoa == selecao.IdProfissional
                        && p.IdEstabelecimento == idEstabelecimento)
                .SelectMany(p => p.Servicoprofissionals
                    .Where(sp => sp.IdEstabelecimento == idEstabelecimento
                            && servicosEscolhidos.Contains(sp.IdServico))
                    .Select(sp => sp.IdServico))
                .ToList();

            return servicosEscolhidos.Count() == servicosProfissional.Count();
        }

        /// <summary>
        /// Obtém os intervalos de tempo livres no turno.
        /// </summary>
        /// <param name="inicioTurno">Início do turno de um profissional.</param>
        /// <param name="fimTurno">Fim do turno de um profissional.</param>
        /// <param name="ocupados">Agendamentos com horários do dia ocupados.</param>
        /// <returns>Id do agendamento criado.</returns>
        private static IEnumerable<(DateTime Inicio, DateTime Fim)> GetIntervalosLivres(DateTime inicioTurno, DateTime fimTurno, IEnumerable<(DateTime Inicio, DateTime Fim)> ocupados)
        {
            var livres = new List<(DateTime, DateTime)>();
            var cursor = inicioTurno;

            var doTurno = ocupados
                .Where(o => o.Inicio < fimTurno && o.Fim > inicioTurno)
                .OrderBy(o => o.Inicio);

            foreach (var (inicio, fim) in doTurno)
            {
                if (inicio > cursor)
                {
                    livres.Add((cursor, inicio));
                }
                if (fim > cursor)
                {
                    cursor = fim;
                }
            }

            if (cursor < fimTurno)
            {
                livres.Add((cursor, fimTurno));
            }

            return livres;
        }

        /// <summary>
        /// Obter a duração total do conjunto de serviços selecionados.
        /// </summary>
        /// <param name="selecao">Profissional, serviços, data e horário selecionado do cliente.</param>
        /// <param name="idEstabelecimento">Id do estabelecimento que o agendamento será feito.</param>
        /// <returns>Duração total em minutos dos serviços.</returns>
        private uint GetDuracaoTotalEmMinutos(SelecaoAgendamentoDTO selecao, uint idEstabelecimento)
        {
            var ids = selecao.IdServicosSelecionados.ToList();

            var duracoes = context.Servicoprofissionals
                .AsNoTracking()
                .Where(sp => sp.IdProfissional == selecao.IdProfissional
                          && sp.IdEstabelecimento == idEstabelecimento
                          && ids.Contains(sp.IdServico))
                .Select(sp => sp.Duracao)
                .ToList();

            return (uint) duracoes.Sum(d => (long) d);
        }
    }
}
