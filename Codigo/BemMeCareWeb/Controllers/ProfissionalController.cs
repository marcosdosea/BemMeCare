using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class ProfissionalController : Controller
    {
        private readonly IProfissionalService profissionalService;

        public ProfissionalController(IProfissionalService profissionalService)
        {
            this.profissionalService = profissionalService;
        }

        // GET: ProfissionalController
        public ActionResult Index()
        {
            var profissionais = profissionalService.GetAll();
            var profissionaisViewModel = ProfissionalMapper.ToViewModel(profissionais);
            return View(profissionaisViewModel);
        }

        // GET: ProfissionalController/Details/5
        public ActionResult Details(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional != null ? ProfissionalMapper.ToViewModel(profissional) : new ProfissionalViewModel();
            return View(profissionalViewModel);
        }

        // GET: ProfissionalController/Create
        public ActionResult Create()
        {
            var profissionalViewModel = new ProfissionalViewModel();
            return View(profissionalViewModel);
        }

        // POST: ProfissionalController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfissionalViewModel profissionalViewModel)
        {
            if (ModelState.IsValid)
            {
                var profissional = ProfissionalMapper.ToEntity(profissionalViewModel);
                profissionalService.Create(profissional);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProfissionalController/Edit/5
        public ActionResult Edit(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional != null ? ProfissionalMapper.ToViewModel(profissional) : new ProfissionalViewModel();
            return View(profissionalViewModel);
        }

        // POST: ProfissionalController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfissionalViewModel profissionalViewModel)
        {
            if (ModelState.IsValid)
            {
                var profissional = ProfissionalMapper.ToEntity(profissionalViewModel);
                profissionalService.Edit(profissional);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProfissionalController/Delete/5
        public ActionResult Delete(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional != null ? ProfissionalMapper.ToViewModel(profissional) : new ProfissionalViewModel();
            return View(profissionalViewModel);
        }

        // POST: ProfissionalController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ProfissionalViewModel profissionalViewModel)
        {
            profissionalService.Delete(profissionalViewModel.IdPessoa);
            return RedirectToAction(nameof(Index));
        }
    }
}