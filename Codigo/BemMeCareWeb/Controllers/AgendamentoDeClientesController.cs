using Core;
using Core.Service;
using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Controllers
{
    public class AgendamentoDeClientesController : Controller
    {
        private const uint IdEstabelecimentoProvisorio = 1;
        private const uint IdPessoaMarcouProvisorio = 1;

        private readonly IAgendamentoDeClientesService agendamentoService;
        private readonly IPessoaService pessoaService;
        private readonly IServicoService servicoService;
        private readonly IProfissionalService profissionalService;

        public AgendamentoDeClientesController(
            IAgendamentoDeClientesService agendamentoService,
            IPessoaService pessoaService,
            IServicoService servicoService,
            IProfissionalService profissionalService)
        {
            this.agendamentoService = agendamentoService;
            this.pessoaService = pessoaService;
            this.servicoService = servicoService;
            this.profissionalService = profissionalService;
        }

        public IActionResult Index(DateTime? data = null, uint? idProfissional = null)
        {
            var dataSelecionada = data?.Date ?? DateTime.Today;
            var agendamentos = agendamentoService.GetByData(
                dataSelecionada,
                IdEstabelecimentoProvisorio,
                idProfissional);

            return View(agendamentos.ToViewModel().ToList());
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AgendamentoDeClientesViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RetornarCreate(viewModel);
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
                return RetornarCreate(viewModel);
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
                return RetornarCreate(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(uint id)
        {
            try
            {
                agendamentoService.Cancel(id, IdEstabelecimentoProvisorio);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarSinal(uint id)
        {
            try
            {
                agendamentoService.ConfirmarSinal(id, IdEstabelecimentoProvisorio);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
