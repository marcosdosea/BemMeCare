using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class EstabelecimentoController : Controller
    {
        private readonly IEstabelecimentoService estabelecimentoService;
        private readonly IPessoaService pessoaService;

        public EstabelecimentoController(IEstabelecimentoService estabelecimentoService, IPessoaService pessoaService)
        {
            this.estabelecimentoService = estabelecimentoService;
            this.pessoaService = pessoaService;
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
                var pessoa = pessoaService.GetAll().FirstOrDefault();
                if (pessoa == null)
                {
                    ModelState.AddModelError(string.Empty, "Não existe nenhuma Pessoa cadastrada no banco de dados para vincular provisoriamente ao estabelecimento. Cadastre pelo menos uma pessoa primeiro.");
                    return View(estabelecimentoViewModel);
                }

                var estabelecimento = EstabelecimentoMapper.ToEntity(estabelecimentoViewModel);
                estabelecimento.IdPessoa = pessoa.Id;
                estabelecimentoService.Create(estabelecimento);
                return RedirectToAction(nameof(Index));
            }
            return View(estabelecimentoViewModel);
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
                if (estabelecimento.IdPessoa == 0)
                {
                    var pessoa = pessoaService.GetAll().FirstOrDefault();
                    if (pessoa != null)
                    {
                        estabelecimento.IdPessoa = pessoa.Id;
                    }
                }
                estabelecimentoService.Edit(estabelecimento);
                return RedirectToAction(nameof(Index));
            }
            return View(estabelecimentoViewModel);
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
