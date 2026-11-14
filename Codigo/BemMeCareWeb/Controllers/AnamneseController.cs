using BemMeCareWeb.Models;
using Core;
using Core.Service;
using BemMeCareWeb.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class AnamneseController : Controller
    {
        // ID provisório que depois será obtido automaticamente pelo usuário logado no sistema
        private const uint idEstabelecimentoProvisorio = 1;
        private readonly IProfissionalService profissionalService;

        public AnamneseController(IProfissionalService profissionalService)
        {
            this.profissionalService = profissionalService;
        }

        // GET: AnamneseController
        public ActionResult Index(uint idProfissional)
        {
            var listaServicos = profissionalService.GetServicoProfissional(idProfissional);
            var listaAnamneseViewModel = listaServicos.ToViewModel();
            return View(listaAnamneseViewModel);
        }

        // GET: AnamneseController/Details/5
        public ActionResult Details(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var servico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var anamneseViewModel = servico != null ? servico.ToViewModel() : new AnamneseViewModel();
            return View(anamneseViewModel);
        }

        // GET: AnamneseController/Create
        public ActionResult Create(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var servico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var anamneseViewModel = servico != null
                ? servico.ToViewModel()
                : new AnamneseViewModel { IdProfissional = idProfissional, IdServico = idServico };
            return View(anamneseViewModel);
        }

        // POST: AnamneseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AnamneseViewModel anamneseViewModel)
        {
            if (ModelState.IsValid)
            {
                anamneseViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;
                var servico = anamneseViewModel.ToEntity();
                profissionalService.EditServicoProfissional(
                    anamneseViewModel.IdProfissional,
                    new List<Servicoprofissional> { servico });
            }
            return RedirectToAction(nameof(Index), new { idProfissional = anamneseViewModel.IdProfissional });
        }

        // GET: AnamneseController/Delete/5
        public ActionResult Delete(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var servico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var anamneseViewModel = servico != null ? servico.ToViewModel() : new AnamneseViewModel();
            return View(anamneseViewModel);
        }

        // POST: AnamneseController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(AnamneseViewModel anamneseViewModel)
        {
            anamneseViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;
            anamneseViewModel.FormAnamnese = string.Empty;
            var servico = anamneseViewModel.ToEntity();
            profissionalService.EditServicoProfissional(
                anamneseViewModel.IdProfissional,
                new List<Servicoprofissional> { servico });
            return RedirectToAction(nameof(Index), new { idProfissional = anamneseViewModel.IdProfissional });
        }
    }
}