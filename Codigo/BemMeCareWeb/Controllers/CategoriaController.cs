using BemMeCareWeb.Models;
using Core.Service;
using BemMeCareWeb.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            this.categoriaService = categoriaService;
        }
        
        // GET: CategoriaController
        public ActionResult Index()
        {
            var listaCategorias = categoriaService.GetAll();
            var listaCategoriaViewModel = listaCategorias.ToViewModel();
            return View(listaCategoriaViewModel);
        }
        
        // GET: CategoriaController/Details/5
        public ActionResult Details(uint id)
        {
            var categoria = categoriaService.Get(id);
            var categoriaViewModel = categoria.ToViewModel();
            return View(categoriaViewModel);
        }
        
        // GET: CategoriaController/Create
        public ActionResult Create()
        {
            var categoriaViewModel = new CategoriaViewModel();
            return View(categoriaViewModel);
        }
        
        // POST: CategoriaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoriaViewModel categoriaViewModel)
        {
            if (ModelState.IsValid)
            {
                var categoria = categoriaViewModel.ToEntity();
                categoriaService.Create(categoria);
            }
            return RedirectToAction(nameof(Index));
        }
        
        // GET: CategoriaController/Edit/5
        public ActionResult Edit(uint id)
        {
            var categoria = categoriaService.Get(id);
            var categoriaViewModel = categoria.ToViewModel();
            return View(categoriaViewModel);
        }
        
        // POST: CategoriaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoriaViewModel categoriaViewModel)
        {
            if (ModelState.IsValid)
            {
                var categoria = categoriaViewModel.ToEntity();
                categoriaService.Edit(categoria);
            }
            return RedirectToAction(nameof(Index));
        }
        
        // GET: CategoriaController/Delete/5
        public ActionResult Delete(uint id)
        {
            var categoria = categoriaService.Get(id);
            var categoriaViewModel = categoria.ToViewModel();
            return View(categoriaViewModel);
        }
        
        // POST: CategoriaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(CategoriaViewModel categoriaViewModel)
        {
            categoriaService.Delete(categoriaViewModel.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}
