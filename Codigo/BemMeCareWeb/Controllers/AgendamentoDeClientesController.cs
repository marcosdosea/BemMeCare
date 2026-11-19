using Core;
using Core.Service;
using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class AgendamentoDeClientesController : Controller
    {
        private const uint IdEstabelecimentoProvisorio = 1;
        private const uint IdPessoaMarcouProvisorio = 1;

        private readonly IAgendamentoService agendamentoService;
        private readonly IPessoaService pessoaService;

        public AgendamentoDeClientesController(
            IAgendamentoService agendamentoService,
            IPessoaService pessoaService)
        {
            this.agendamentoService = agendamentoService;
            this.pessoaService = pessoaService;
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
            return View(new AgendamentoDeClientesViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AgendamentoDeClientesViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
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
                return View(viewModel);
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
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(uint id)
        {
            agendamentoService.Cancel(id, IdEstabelecimentoProvisorio);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarSinal(uint id)
        {
            agendamentoService.ConfirmarSinal(id, IdEstabelecimentoProvisorio);
            return RedirectToAction(nameof(Index));
        }

        private static string NormalizarTelefone(string telefone)
        {
            return new string(telefone.Where(char.IsDigit).ToArray());
        }
    }
}
