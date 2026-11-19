using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class AgendamentoController : Controller
    {
        // ID provisório que depois será obtido automaticamente pelo usuário logado no sistema
        private const uint idEstabelecimentoProvisorio = 1;
        private const uint idPessoaProvisorio = 1;
        private readonly IProfissionalService profissionalService;
        private readonly IAgendamentoService agendamentoService;
        private readonly IHorarioService horarioService;
        private readonly IPessoaService pessoaService;

        public AgendamentoController(IProfissionalService profissionalService, IAgendamentoService agendamentoService, IHorarioService horarioService, IPessoaService pessoaService)
        {
            this.profissionalService = profissionalService;
            this.agendamentoService = agendamentoService;
            this.horarioService = horarioService;
            this.pessoaService = pessoaService;
        }

        // GET: Agendamento/Servico
        public ActionResult Servico(SelecaoAgendamentoViewModel selecao)
        {
            uint idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicosProfissionais(idEstabelecimento);
            var agendamentoServicosViewModel = profissionalServico.ToViewModel();
            agendamentoServicosViewModel.Selecao = selecao;

            return View(agendamentoServicosViewModel);
        }

        // GET: Agendamento/Horario
        public ActionResult Horario(SelecaoAgendamentoViewModel selecao)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Os dados informados são inválidos. Escolha os serviços novamente.";
                return RedirectToAction(nameof(Servico));
            }

            var idEstabelecimento = idEstabelecimentoProvisorio;

            var hoje = DateOnly.FromDateTime(DateTime.Now);
            if (selecao.DataDoAgendamento is null || selecao.DataDoAgendamento < hoje)
            {
                selecao.DataDoAgendamento = hoje;
            }
            selecao.HorarioAgendamento = null;

            var selecaoDTO = selecao.ToEntity();

            if (!agendamentoService.ValidateProfissionalServicosAgendamento(selecaoDTO, idEstabelecimento))
            {
                TempData["Erro"] = "A seleção de serviços não é válida. Escolha novamente.";
                return RedirectToAction(nameof(Servico));
            }

            var viewModel = new AgendamentoHorarioViewModel
            {
                Selecao = selecao,
                NomeProfissional = pessoaService.GetNome(selecao.IdProfissional),
                DataLimiteInferior = hoje
            };

            var horariosProfissional = horarioService.GetByProfissionalNaData(selecao.IdProfissional, selecao.DataDoAgendamento.Value, idEstabelecimento);

            if (!horariosProfissional.Any())
            {
                viewModel.Mensagem = "O profissional não atende nesta data.";
                return View(viewModel);
            }

            var horariosDisponiveis = agendamentoService.GetHorariosDisponiveisProfissionalAgendamento(
                selecaoDTO, horariosProfissional, idEstabelecimento);

            viewModel.Horarios = horariosDisponiveis;

            if (!horariosDisponiveis.Any())
            {
                viewModel.Mensagem = "Não há horários disponíveis nesta data.";
            }

            return View(viewModel);

        }

        // GET: Agendamento/Resumo
        public ActionResult Resumo(SelecaoAgendamentoViewModel selecao)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Os dados informados são inválidos. Escolha os serviços novamente.";
                return RedirectToAction(nameof(Servico));
            }

            var idEstabelecimento = idEstabelecimentoProvisorio;
            var selecaoDTO = selecao.ToEntity();

            if (!agendamentoService.ValidateProfissionalServicosAgendamento(selecaoDTO, idEstabelecimento))
            {
                TempData["Erro"] = "A seleção de serviços não é válida. Escolha novamente.";
                return RedirectToAction(nameof(Servico));
            }

            if (selecao.DataDoAgendamento is null || selecao.HorarioAgendamento is null)
            {
                TempData["Erro"] = "Escolha a data e o horário do atendimento.";
                selecao.HorarioAgendamento = null;
                return RedirectToAction(nameof(Horario), selecao);
            }

            var horariosProfissional = horarioService.GetByProfissionalNaData(selecao.IdProfissional, selecao.DataDoAgendamento.Value, idEstabelecimento);

            var horarioDisponivel = horariosProfissional.Any()
                && agendamentoService
                    .GetHorariosDisponiveisProfissionalAgendamento(selecaoDTO, horariosProfissional, idEstabelecimento)
                    .Contains(selecao.HorarioAgendamento.Value);

            if (!horarioDisponivel)
            {
                TempData["Erro"] = "O horário escolhido não está mais disponível. Escolha outro.";
                selecao.HorarioAgendamento = null;
                return RedirectToAction(nameof(Horario), selecao);
            }

            var resumo = agendamentoService.GetResumoAgendamento(selecaoDTO, idEstabelecimento);
            var viewModel = resumo.ToViewModel();
            viewModel.Selecao = selecao;

            return View(viewModel);
        }

        // GET: AgendamentoController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AgendamentoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // POST: AgendamentoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SelecaoAgendamentoViewModel selecao)
        {
            if (!ModelState.IsValid
                || selecao.DataDoAgendamento is null
                || selecao.HorarioAgendamento is null)
            {
                TempData["Erro"] = "Os dados do agendamento são inválidos. Escolha novamente.";
                return RedirectToAction(nameof(Servico));
            }

            var idEstabelecimento = idEstabelecimentoProvisorio;
            var idPessoa = idPessoaProvisorio;
            var selecaoDTO = selecao.ToEntity();

            var resultado = agendamentoService.Create(selecaoDTO, idEstabelecimento, idPessoa);

            // Sucesso: manda de volta para começo do processo de agendamento provisoriamente
            return RedirectToAction(nameof(Servico));
        }

        // GET: AgendamentoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AgendamentoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AgendamentoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AgendamentoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
