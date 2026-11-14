using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace BemMeCareWeb.Controllers
{
    public class HorarioController : Controller
    {
        // ID provisório que depois será obtido automaticamente pelo usuário logado no sistema
        private const uint IdEstabelecimentoProvisorio = 1;
        private readonly IHorarioService horarioService;

        public HorarioController(IHorarioService horarioService)
        {
            this.horarioService = horarioService;
        }

        // GET: HorarioController
        public ActionResult Index(uint? idProfissional = null, string? diaSemana = null)
        {
            var idEstabelecimento = IdEstabelecimentoProvisorio;
            IEnumerable<Core.Horario> horarios;

            if (idProfissional.HasValue && !string.IsNullOrWhiteSpace(diaSemana))
            {
                horarios = horarioService.GetByDiaSemana(diaSemana, idProfissional.Value, idEstabelecimento);
            }
            else if (idProfissional.HasValue)
            {
                horarios = horarioService.GetByProfissional(idProfissional.Value, idEstabelecimento);
            }
            else
            {
                horarios = horarioService.GetAll(idEstabelecimento);
            }

            var listaHorarioViewModel = horarios.ToViewModel();
            return View(listaHorarioViewModel);
        }

        // GET: HorarioController/Details/5
        public ActionResult Details(uint id)
        {
            var idEstabelecimento = IdEstabelecimentoProvisorio;
            var horario = horarioService.Get(id, idEstabelecimento);
            var horarioViewModel = horario != null ? horario.ToViewModel() : new HorarioViewModel();
            return View(horarioViewModel);
        }

        // GET: HorarioController/Create
        public ActionResult Create()
        {
            var horarioViewModel = new HorarioViewModel
            {
                InicioVigencia = DateTime.Today
            };
            return View(horarioViewModel);
        }

        // POST: HorarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HorarioViewModel horarioViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    horarioViewModel.IdEstabelecimento = IdEstabelecimentoProvisorio;
                    var horario = horarioViewModel.ToEntity();
                    horarioService.Create(horario);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(horarioViewModel);
        }

        // GET: HorarioController/Edit/5
        public ActionResult Edit(uint id)
        {
            var idEstabelecimento = IdEstabelecimentoProvisorio;
            var horario = horarioService.Get(id, idEstabelecimento);
            var horarioViewModel = horario != null ? horario.ToViewModel() : new HorarioViewModel();
            return View(horarioViewModel);
        }

        // POST: HorarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HorarioViewModel horarioViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    horarioViewModel.IdEstabelecimento = IdEstabelecimentoProvisorio;
                    var horario = horarioViewModel.ToEntity();
                    horarioService.Edit(horario, horarioViewModel.IdEstabelecimento);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(horarioViewModel);
        }

        // GET: HorarioController/Delete/5
        public ActionResult Delete(uint id)
        {
            var idEstabelecimento = IdEstabelecimentoProvisorio;
            var horario = horarioService.Get(id, idEstabelecimento);
            var horarioViewModel = horario != null ? horario.ToViewModel() : new HorarioViewModel();
            return View(horarioViewModel);
        }

        // POST: HorarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(HorarioViewModel horarioViewModel)
        {
            horarioViewModel.IdEstabelecimento = IdEstabelecimentoProvisorio;
            horarioService.Delete(horarioViewModel.Id, horarioViewModel.IdEstabelecimento);
            return RedirectToAction(nameof(Index));
        }
    }
}
