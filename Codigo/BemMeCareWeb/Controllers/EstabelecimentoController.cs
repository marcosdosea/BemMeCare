using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BemMeCareWeb.Controllers
{
    public class EstabelecimentoController : Controller
    {
        private readonly IEstabelecimentoService estabelecimentoService;
        public EstabelecimentoController(IEstabelecimentoService estabelecimentoService)
        {
            this.estabelecimentoService = estabelecimentoService;
        }

        // GET: EstabelecimentoController
        public ActionResult Index()
        {
            var listaEstabelecimentos = estabelecimentoService.GetAll();
            var listaEstabelecimentoViewModel = listaEstabelecimentos.ToViewModel();
            return View(listaEstabelecimentoViewModel);
        }

        // GET: EstabelecimentoController/Details/5
        public ActionResult Details(uint id)
        {
            var estabelecimento = estabelecimentoService.Get(id);
            var estabelecimentoViewModel = estabelecimento != null ? EstabelecimentoMapper.ToViewModel(estabelecimento) : new EstabelecimentoViewModel();
            return View(estabelecimentoViewModel);
        }

        // GET: EstabelecimentoController/Create
        public ActionResult Create()
        {
            var estabelecimentoViewModel = new EstabelecimentoViewModel();
            return View(estabelecimentoViewModel);
        }

        // POST: EstabelecimentoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EstabelecimentoViewModel estabelecimentoViewModel)
        {
            if (ModelState.IsValid)
            {
                var estabelecimento = EstabelecimentoMapper.ToEntity(estabelecimentoViewModel);
                estabelecimentoService.Create(estabelecimento);
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: EstabelecimentoController/Edit/5
        public ActionResult Edit(uint id)
        {
            var estabelecimento = estabelecimentoService.Get(id);
            var estabelecimentoViewModel = estabelecimento != null ? EstabelecimentoMapper.ToViewModel(estabelecimento) : new EstabelecimentoViewModel();
            return View(estabelecimentoViewModel);
        }
        // POST: EstabelecimentoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EstabelecimentoViewModel estabelecimentoViewModel)
        {
            if (ModelState.IsValid)
            {
                var estabelecimento = EstabelecimentoMapper.ToEntity(estabelecimentoViewModel);
                estabelecimentoService.Edit(estabelecimento);
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: EstabelecimentoController/Delete/5
        public ActionResult Delete(uint id)
        {
            var estabelecimento = estabelecimentoService.Get(id);
            var estabelecimentoViewModel = estabelecimento != null ? EstabelecimentoMapper.ToViewModel(estabelecimento) : new EstabelecimentoViewModel();
            return View(estabelecimentoViewModel);
        }
        // POST: EstabelecimentoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EstabelecimentoViewModel estabelecimentoViewModel)
        {
            estabelecimentoService.Delete(estabelecimentoViewModel.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}