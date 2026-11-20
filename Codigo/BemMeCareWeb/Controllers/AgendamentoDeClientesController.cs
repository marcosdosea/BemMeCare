using System.Globalization;
using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Controllers
{
    public class AgendamentoDeClientesController : Controller
    {
        private const uint IdEstabelecimentoProvisorio = 1;
        private const uint IdPessoaMarcouProvisorio = 1;
        private const string VisualizacaoMes = "mes";
        private const string VisualizacaoSemana = "semana";
        private const string VisualizacaoDia = "dia";

        private readonly IAgendamentoDeClientesService agendamentoService;
        private readonly IPessoaService pessoaService;
        private readonly IProfissionalService profissionalService;

        public AgendamentoDeClientesController(
            IAgendamentoDeClientesService agendamentoService,
            IPessoaService pessoaService,
            IProfissionalService profissionalService)
        {
            this.agendamentoService = agendamentoService;
            this.pessoaService = pessoaService;
            this.profissionalService = profissionalService;
        }

        public IActionResult Index(
            DateTime? data = null,
            string visualizacao = VisualizacaoMes,
            uint? idProfissional = null)
        {
            if (visualizacao is not VisualizacaoMes and not VisualizacaoSemana and not VisualizacaoDia)
            {
                return BadRequest();
            }

            var dataReferencia = (data ?? DateTime.Today).Date;
            var (dataInicio, dataFimExclusiva, tituloPeriodo) =
                ObterPeriodo(dataReferencia, visualizacao);
            var agendamentos = agendamentoService.GetByPeriodo(
                dataInicio,
                dataFimExclusiva,
                IdEstabelecimentoProvisorio,
                idProfissional)
                .ToViewModel()
                .GroupBy(agendamento => agendamento.DataHoraAtendimento!.Value.Date)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.ToList());

            var dias = new List<AgendamentoDiaViewModel>();
            for (var dia = dataInicio; dia < dataFimExclusiva; dia = dia.AddDays(1))
            {
                agendamentos.TryGetValue(dia, out var agendamentosDoDia);
                dias.Add(new AgendamentoDiaViewModel
                {
                    Data = dia,
                    PertenceAoMesAtual = dia.Month == dataReferencia.Month &&
                        dia.Year == dataReferencia.Year,
                    Agendamentos = agendamentosDoDia ?? []
                });
            }

            var tamanhoSemana = visualizacao == VisualizacaoDia ? 1 : 7;
            var semanas = dias
                .Chunk(tamanhoSemana)
                .Select(semana => (IReadOnlyList<AgendamentoDiaViewModel>)semana)
                .ToList();

            return View(new AgendamentoDeClientesAgendaViewModel
            {
                DataReferencia = dataReferencia,
                DataAnterior = AvancarPeriodo(dataReferencia, visualizacao, -1),
                DataProxima = AvancarPeriodo(dataReferencia, visualizacao, 1),
                Visualizacao = visualizacao,
                TituloPeriodo = tituloPeriodo,
                IdProfissional = idProfissional,
                ProfissionaisDisponiveis = ObterProfissionaisDisponiveis(idProfissional),
                Semanas = semanas
            });
        }

        public IActionResult Details(uint id)
        {
            var agendamento = agendamentoService.Get(id, IdEstabelecimentoProvisorio);
            if (agendamento == null)
            {
                return NotFound();
            }

            return View(agendamento.ToViewModel());
        }

        public IActionResult Create()
        {
            var viewModel = new AgendamentoDeClientesViewModel();
            PopularOpcoesCreate(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Horarios(uint idProfissional, uint idServico, DateTime data)
        {
            if (idProfissional == 0 || idServico == 0 || data.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Escolha o serviço, o profissional e uma data válida.";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                var viewModel = new AgendamentoDeClientesViewModel
                {
                    IdProfissional = idProfissional,
                    IdsServicos = [idServico],
                    DataHoraAtendimento = data.Date,
                    HorariosDisponiveis = agendamentoService.GetHorariosDisponiveis(
                        idProfissional,
                        idServico,
                        data.Date,
                        IdEstabelecimentoProvisorio)
                };
                PopularOpcoesCreate(viewModel);

                var opcao = viewModel.OpcoesServicoProfissional.FirstOrDefault(item =>
                    item.IdProfissional == idProfissional &&
                    item.IdServico == idServico);
                viewModel.NomeProfissional = opcao?.NomeProfissional;
                viewModel.NomesServicos = opcao?.NomeServico;

                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        [HttpGet]
        public IActionResult Resumo(
            uint idProfissional,
            uint idServico,
            DateTime dataHoraAtendimento)
        {
            if (idProfissional == 0 || idServico == 0 || dataHoraAtendimento == default)
            {
                TempData["ErrorMessage"] = "Selecione um horário válido.";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                var resumo = agendamentoService.CriarResumo(
                    idProfissional,
                    idServico,
                    dataHoraAtendimento,
                    IdEstabelecimentoProvisorio);
                var viewModel = new AgendamentoDeClientesViewModel
                {
                    IdProfissional = idProfissional,
                    IdsServicos = [idServico],
                    DataHoraAtendimento = resumo.HorarioAtendimento,
                    PrecoFinal = resumo.PrecoTotal,
                    DuracaoMinutos = resumo.DuracaoTotalEmMinutos,
                    NomeProfissional = resumo.NomeProfissional,
                    NomesServicos = string.Join(", ", resumo.Servicos)
                };

                var opcoes = PopularOpcoesCreateRetornando(viewModel);
                var opcao = opcoes.FirstOrDefault(item =>
                    item.IdProfissional == idProfissional &&
                    item.IdServico == idServico);
                viewModel.NomeProfissional = opcao?.NomeProfissional;
                viewModel.NomesServicos = opcao?.NomeServico;

                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AgendamentoDeClientesViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RetornarResumo(viewModel);
            }

            var pessoaCliente = pessoaService
                .GetByNome(viewModel.NomeCliente.Trim())
                .FirstOrDefault(pessoa =>
                    string.Equals(
                        pessoa.Nome.Trim(),
                        viewModel.NomeCliente.Trim(),
                        StringComparison.OrdinalIgnoreCase) &&
                    NormalizarTelefone(pessoa.Telefone) == NormalizarTelefone(viewModel.TelefoneCliente));

            if (pessoaCliente == null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.NomeCliente),
                    "Não encontramos uma pessoa com esse nome e telefone. Cadastre a pessoa antes de criar o agendamento.");
                return RetornarResumo(viewModel);
            }

            var agendamento = viewModel.ToEntity();
            agendamento.IdPessoaCliente = pessoaCliente.Id;
            agendamento.IdEstabelecimento = IdEstabelecimentoProvisorio;
            agendamento.IdPessoaMarcou = IdPessoaMarcouProvisorio;
            agendamento.DataHoraCriacao = DateTime.Now;

            try
            {
                agendamentoService.Create(agendamento, viewModel.IdsServicos);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RetornarResumo(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(
            uint id,
            DateTime? data = null,
            string visualizacao = VisualizacaoMes,
            uint? idProfissional = null)
        {
            try
            {
                agendamentoService.Cancel(id, IdEstabelecimentoProvisorio);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { data, visualizacao, idProfissional });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarSinal(
            uint id,
            DateTime? data = null,
            string visualizacao = VisualizacaoMes,
            uint? idProfissional = null)
        {
            try
            {
                agendamentoService.ConfirmarSinal(id, IdEstabelecimentoProvisorio);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { data, visualizacao, idProfissional });
        }

        private IActionResult RetornarCreate(AgendamentoDeClientesViewModel viewModel)
        {
            PopularOpcoesCreate(viewModel);
            return View(nameof(Create), viewModel);
        }

        private IActionResult RetornarResumo(AgendamentoDeClientesViewModel viewModel)
        {
            var opcoes = PopularOpcoesCreateRetornando(viewModel);
            var idServico = viewModel.IdsServicos.FirstOrDefault();
            var opcao = opcoes.FirstOrDefault(item =>
                item.IdProfissional == viewModel.IdProfissional &&
                item.IdServico == idServico);
            viewModel.NomeProfissional = opcao?.NomeProfissional;
            viewModel.NomesServicos = opcao?.NomeServico;

            if (viewModel.DataHoraAtendimento.HasValue &&
                viewModel.IdProfissional > 0 &&
                idServico > 0)
            {
                try
                {
                    var resumo = agendamentoService.CriarResumo(
                        viewModel.IdProfissional,
                        idServico,
                        viewModel.DataHoraAtendimento.Value,
                        IdEstabelecimentoProvisorio);
                    if (resumo != null)
                    {
                        viewModel.PrecoFinal = resumo.PrecoTotal;
                        viewModel.DuracaoMinutos = resumo.DuracaoTotalEmMinutos;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(nameof(Resumo), viewModel);
        }

        private void PopularOpcoesCreate(AgendamentoDeClientesViewModel viewModel)
        {
            viewModel.OpcoesServicoProfissional = PopularOpcoesCreateRetornando(viewModel);
        }

        private IEnumerable<ServicoProfissionalOpcaoViewModel> PopularOpcoesCreateRetornando(
            AgendamentoDeClientesViewModel viewModel)
        {
            var nomesPessoas = pessoaService.GetAll()
                .ToDictionary(pessoa => pessoa.Id, pessoa => pessoa.Nome);
            var opcoes = profissionalService.GetAll()
                .Where(profissional =>
                    profissional.IdEstabelecimento == IdEstabelecimentoProvisorio &&
                    profissional.EstaAtivo == 1)
                .SelectMany(profissional => profissional.Servicoprofissionals
                    .Where(servicoProfissional =>
                        servicoProfissional.IdEstabelecimento == IdEstabelecimentoProvisorio)
                    .Select(servicoProfissional => new ServicoProfissionalOpcaoViewModel
                    {
                        IdServico = servicoProfissional.IdServico,
                        IdProfissional = profissional.IdPessoa,
                        NomeServico = servicoProfissional.IdServicoNavigation.Nome,
                        NomeProfissional = nomesPessoas.TryGetValue(profissional.IdPessoa, out var nome)
                            ? nome
                            : $"Profissional {profissional.IdPessoa}"
                    }))
                .OrderBy(opcao => opcao.NomeServico)
                .ThenBy(opcao => opcao.NomeProfissional)
                .ToList();
            viewModel.OpcoesServicoProfissional = opcoes;
            return opcoes;
        }

        private IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            ObterProfissionaisDisponiveis(uint? idProfissionalSelecionado)
        {
            var nomesPessoas = pessoaService.GetAll()
                .ToDictionary(pessoa => pessoa.Id, pessoa => pessoa.Nome);
            return profissionalService.GetAll()
                .Where(profissional =>
                    profissional.IdEstabelecimento == IdEstabelecimentoProvisorio &&
                    profissional.EstaAtivo == 1)
                .Select(profissional => new
                {
                    profissional.IdPessoa,
                    Nome = nomesPessoas.TryGetValue(profissional.IdPessoa, out var nome)
                        ? nome
                        : $"Profissional {profissional.IdPessoa}"
                })
                .OrderBy(profissional => profissional.Nome)
                .Select(profissional => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = profissional.IdPessoa.ToString(),
                    Text = profissional.Nome,
                    Selected = profissional.IdPessoa == idProfissionalSelecionado
                })
                .ToList();
        }

        private static (DateTime Inicio, DateTime FimExclusivo, string Titulo) ObterPeriodo(
            DateTime dataReferencia,
            string visualizacao)
        {
            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            if (visualizacao == VisualizacaoMes)
            {
                var primeiroDiaDoMes = new DateTime(dataReferencia.Year, dataReferencia.Month, 1);
                var diasAntes = ((int)primeiroDiaDoMes.DayOfWeek + 6) % 7;
                var inicio = primeiroDiaDoMes.AddDays(-diasAntes);
                var totalDias = diasAntes + DateTime.DaysInMonth(dataReferencia.Year, dataReferencia.Month);
                var totalCelulas = ((totalDias + 6) / 7) * 7;
                return (
                    inicio,
                    inicio.AddDays(totalCelulas),
                    dataReferencia.ToString("MMMM yyyy", cultura));
            }

            if (visualizacao == VisualizacaoSemana)
            {
                var diasDesdeSegunda = ((int)dataReferencia.DayOfWeek + 6) % 7;
                var inicio = dataReferencia.AddDays(-diasDesdeSegunda);
                var fim = inicio.AddDays(7);
                return (
                    inicio,
                    fim,
                    $"{inicio.ToString("dd MMM", cultura)} – {fim.AddDays(-1).ToString("dd MMM yyyy", cultura)}");
            }

            return (
                dataReferencia,
                dataReferencia.AddDays(1),
                dataReferencia.ToString("dddd, dd MMMM yyyy", cultura));
        }

        private static DateTime AvancarPeriodo(DateTime dataReferencia, string visualizacao, int quantidade)
        {
            return visualizacao switch
            {
                VisualizacaoMes => new DateTime(
                    dataReferencia.Year,
                    dataReferencia.Month,
                    1).AddMonths(quantidade),
                VisualizacaoSemana => dataReferencia.AddDays(7 * quantidade),
                _ => dataReferencia.AddDays(quantidade)
            };
        }

        private IActionResult RetornarCreate(AgendamentoDeClientesViewModel viewModel)
        {
            PopularOpcoesCreate(viewModel);
            return View(nameof(Create), viewModel);
        }

        private void PopularOpcoesCreate(AgendamentoDeClientesViewModel viewModel)
        {
            var pessoasPorId = pessoaService.GetAll()
                .ToDictionary(pessoa => pessoa.Id, pessoa => pessoa.Nome);
            viewModel.ProfissionaisDisponiveis = profissionalService.GetAll()
                .Where(profissional =>
                    profissional.IdEstabelecimento == IdEstabelecimentoProvisorio &&
                    profissional.EstaAtivo == 1)
                .Select(profissional => new SelectListItem
                {
                    Value = profissional.IdPessoa.ToString(),
                    Text = pessoasPorId.TryGetValue(profissional.IdPessoa, out var nome)
                        ? nome
                        : $"Profissional {profissional.IdPessoa}",
                    Selected = profissional.IdPessoa == viewModel.IdProfissional
                })
                .ToList();

            viewModel.ServicosDisponiveis = servicoService.GetAll(IdEstabelecimentoProvisorio)
                .Select(servico => new SelectListItem
                {
                    Value = servico.Id.ToString(),
                    Text = servico.Nome,
                    Selected = viewModel.IdsServicos.Contains(servico.Id)
                })
                .ToList();
        }

        private static string NormalizarTelefone(string telefone)
        {
            return new string(telefone.Where(char.IsDigit).ToArray());
        }
    }
}
