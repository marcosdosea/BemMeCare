using BemMeCareWeb.Models;
using Core.Service;
using BemMeCareWeb.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class ServicoController : Controller
    {
        private readonly IServicoService servicoService;

        public ServicoController(IServicoService servicoService)
        {
            this.servicoService = servicoService;
        }

        // GET: ServicoController
        public ActionResult Index()
        {
            var listaServicos = servicoService.GetAll();
            var listaServicoViewModel = listaServicos.ToViewModel();
            return View(listaServicoViewModel);
        }

        // GET: ServicoController/Details/5
        public ActionResult Details(uint id)
        {
            var servico = servicoService.Get(id);
            var servicoViewModel = servico != null
                ? servico.ToViewModel()
                : new ServicoViewModel();
            return View(servicoViewModel);
        }

        // GET: ServicoController/Create
        public ActionResult Create()
        {
            var servicoViewModel = new ServicoViewModel();
            return View(servicoViewModel);
        }

        // POST: ServicoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServicoViewModel servicoViewModel)
        {
            if (ModelState.IsValid)
            {
                var servico = servicoViewModel.ToEntity();
                servicoService.Create(servico);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServicoController/Edit/5
        public ActionResult Edit(uint id)
        {
            var servico = servicoService.Get(id);
            var servicoViewModel = servico != null
                ? servico.ToViewModel()
                : new ServicoViewModel();
            return View(servicoViewModel);
        }

        // POST: ServicoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServicoViewModel servicoViewModel)
        {
            if (ModelState.IsValid)
            {
                var servico = servicoViewModel.ToEntity();
                servicoService.Edit(servico);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServicoController/Delete/5
        public ActionResult Delete(uint id)
        {
            var servico = servicoService.Get(id);
            var servicoViewModel = servico != null
                ? servico.ToViewModel()
                : new ServicoViewModel();
            return View(servicoViewModel);
        }

        // POST: ServicoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ServicoViewModel servicoViewModel)
        {
            servicoService.Delete(servicoViewModel.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}