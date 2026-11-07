using BemMeCareWeb.Models;
using Core.Service;
using BemMeCareWeb.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class PessoaController : Controller
    {
        private readonly IPessoaService pessoaService;

        public PessoaController(IPessoaService pessoaService)
        {
            this.pessoaService = pessoaService;
        }

        // GET: PessoaController
        public ActionResult Index()
        {
            var listaPessoas = pessoaService.GetAll();
            var listaPessoaViewModel = listaPessoas.ToViewModel();
            return View(listaPessoaViewModel);
        }

        // GET: PessoaController/Details/5
        public ActionResult Details(uint id)
        {
            var pessoa = pessoaService.Get(id);
            var pessoaViewModel = pessoa != null ? pessoa.ToViewModel() : new PessoaViewModel();
            return View(pessoaViewModel);
        }

        // GET: PessoaController/Create
        public ActionResult Create()
        {
            var pessoaViewModel = new PessoaViewModel();
            return View(pessoaViewModel);
        }

        // POST: PessoaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PessoaViewModel pessoaViewModel)
        {
            if (ModelState.IsValid)
            {
                var pessoa = pessoaViewModel.ToEntity();
                pessoaService.Create(pessoa);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: PessoaController/Edit/5
        public ActionResult Edit(uint id)
        {
            var pessoa = pessoaService.Get(id);
            var pessoaViewModel = pessoa != null ? pessoa.ToViewModel() : new PessoaViewModel();
            return View(pessoaViewModel);
        }

        // POST: PessoaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PessoaViewModel pessoaViewModel)
        {
            if (ModelState.IsValid)
            {
                var pessoa = pessoaViewModel.ToEntity();
                pessoaService.Edit(pessoa);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: PessoaController/Delete/5
        public ActionResult Delete(uint id)
        {
            var pessoa = pessoaService.Get(id);
            var pessoaViewModel = pessoa != null ? pessoa.ToViewModel() : new PessoaViewModel();
            return View(pessoaViewModel);
        }

        // POST: PessoaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(PessoaViewModel pessoaViewModel)
        {
            pessoaService.Delete(pessoaViewModel.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}
