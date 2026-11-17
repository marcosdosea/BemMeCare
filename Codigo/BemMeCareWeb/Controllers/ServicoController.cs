using BemMeCareWeb.Models;
using Core;
using Core.Service;
using BemMeCareWeb.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Controllers
{
    public class ServicoController : Controller
    {
        private const uint IdEstabelecimentoProvisorio = 1;
        private readonly IServicoService servicoService;
        private readonly ICategoriaService categoriaService;

        public ServicoController(
            IServicoService servicoService,
            ICategoriaService categoriaService)
        {
            this.servicoService = servicoService;
            this.categoriaService = categoriaService;
        }

        // GET: ServicoController
        public ActionResult Index(string? nome = null)
        {
            var servicos = string.IsNullOrWhiteSpace(nome)
                ? servicoService.GetAll(IdEstabelecimentoProvisorio)
                : servicoService.GetByNome(nome.Trim(), IdEstabelecimentoProvisorio);
            var servicoViewModels = servicos.ToViewModel().ToList();
            var categoriasPorId = GetCategoriasDoEstabelecimento()
                .ToDictionary(categoria => categoria.Id, categoria => categoria.Nome);

            foreach (var servicoViewModel in servicoViewModels)
            {
                servicoViewModel.NomeCategoria =
                    categoriasPorId.TryGetValue(servicoViewModel.IdCategoria, out var nomeCategoria)
                        ? nomeCategoria
                        : "Categoria não encontrada";
            }

            ViewData["NomePesquisado"] = nome?.Trim();
            return View(servicoViewModels);
        }

        // GET: ServicoController/Details/5
        public ActionResult Details(uint id)
        {
            var servico = servicoService.Get(id, IdEstabelecimentoProvisorio);
            var servicoViewModel = servico != null
                ? servico.ToViewModel()
                : new ServicoViewModel();
            return View(servicoViewModel);
        }

        // GET: ServicoController/Create
        public ActionResult Create()
        {
            PopularCategorias();
            var servicoViewModel = new ServicoViewModel();
            return View(servicoViewModel);
        }

        // POST: ServicoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServicoViewModel servicoViewModel)
        {
            if (ModelState.IsValid && CategoriaPertenceAoEstabelecimento(servicoViewModel.IdCategoria))
            {
                var servico = servicoViewModel.ToEntity();
                servicoService.Create(servico, IdEstabelecimentoProvisorio);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServicoController/Edit/5
        public ActionResult Edit(uint id)
        {
            var servico = servicoService.Get(id, IdEstabelecimentoProvisorio);
            var servicoViewModel = servico != null
                ? servico.ToViewModel()
                : new ServicoViewModel();
            PopularCategorias();
            return View(servicoViewModel);
        }

        // POST: ServicoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServicoViewModel servicoViewModel)
        {
            if (ModelState.IsValid && CategoriaPertenceAoEstabelecimento(servicoViewModel.IdCategoria))
            {
                var servico = servicoViewModel.ToEntity();
                servicoService.Edit(servico, IdEstabelecimentoProvisorio);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServicoController/Delete/5
        public ActionResult Delete(uint id)
        {
            var servico = servicoService.Get(id, IdEstabelecimentoProvisorio);
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
            servicoService.Delete(servicoViewModel.Id, IdEstabelecimentoProvisorio);
            return RedirectToAction(nameof(Index));
        }

        private void PopularCategorias()
        {
            ViewBag.Categorias = new SelectList(
                GetCategoriasDoEstabelecimento(),
                nameof(Categorium.Id),
                nameof(Categorium.Nome));
        }

        private bool CategoriaPertenceAoEstabelecimento(uint idCategoria)
        {
            return GetCategoriasDoEstabelecimento()
                .Any(categoria => categoria.Id == idCategoria);
        }

        private IEnumerable<Categorium> GetCategoriasDoEstabelecimento()
        {
            return categoriaService.GetAll(IdEstabelecimentoProvisorio);
        }
    }
}
