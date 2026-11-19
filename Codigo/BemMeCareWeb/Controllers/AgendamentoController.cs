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
        private readonly IProfissionalService profissionalService;
        private readonly IAgendamentoService agendamentoService;

        public AgendamentoController(IProfissionalService profissionalService, IAgendamentoService agendamentoService)
        {
            this.profissionalService = profissionalService;
            this.agendamentoService = agendamentoService;
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

        // GET: AgendamentoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AgendamentoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
