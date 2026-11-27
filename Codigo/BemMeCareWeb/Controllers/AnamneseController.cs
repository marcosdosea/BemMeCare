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
        private readonly IServicoService servicoService;

        public AnamneseController(IProfissionalService profissionalService, IServicoService servicoService)
        {
            this.profissionalService = profissionalService;
            this.servicoService = servicoService;
        }

        // GET: AnamneseController
        public ActionResult Index(uint idProfissional)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var listaProfissionalServicos = profissionalService.GetServicoProfissional(idProfissional, idEstabelecimento);
            var listaProfissionalServicoViewModel = listaProfissionalServicos.ToViewModel();
            ViewBag.IdProfissional = idProfissional;
            ViewBag.NomesServicos = servicoService.GetAll(idEstabelecimento)
                .ToDictionary(servico => servico.Id, servico => servico.Nome);
            return View(listaProfissionalServicoViewModel);
        }

        // GET: AnamneseController/Details/5
        public ActionResult Details(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var profissionalServicoViewModel = profissionalServico != null ? profissionalServico.ToViewModel() : new ProfissionalServicoViewModel();
            return View(profissionalServicoViewModel);
        }

        // GET: AnamneseController/Create
        public ActionResult Create(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var profissionalServicoViewModel = profissionalServico != null
                ? profissionalServico.ToViewModel()
                : new ProfissionalServicoViewModel { IdProfissional = idProfissional, IdServico = idServico };
            return View(profissionalServicoViewModel);
        }

        // POST: AnamneseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfissionalServicoViewModel profissionalServicoViewModel)
        {
            if (ModelState.IsValid)
            {
                profissionalServicoViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;
                profissionalServicoViewModel.ExigeAnamnese = true;
                var profissionalServico = profissionalServicoViewModel.ToEntity();
                profissionalService.EditServicoProfissional(
                    profissionalServicoViewModel.IdProfissional,
                    new List<Servicoprofissional> { profissionalServico });
            }
            return RedirectToAction(nameof(Index), new { idProfissional = profissionalServicoViewModel.IdProfissional });
        }

        // GET: AnamneseController/Delete/5
        public ActionResult Delete(uint idProfissional, uint idServico)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);
            var profissionalServicoViewModel = profissionalServico != null ? profissionalServico.ToViewModel() : new ProfissionalServicoViewModel();
            return View(profissionalServicoViewModel);
        }

        // POST: AnamneseController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ProfissionalServicoViewModel profissionalServicoViewModel)
        {
            profissionalServicoViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;
            profissionalServicoViewModel.FormAnamnese = string.Empty;
            profissionalServicoViewModel.ExigeAnamnese = false;
            var profissionalServico = profissionalServicoViewModel.ToEntity();
            profissionalService.EditServicoProfissional(
                profissionalServicoViewModel.IdProfissional,
                new List<Servicoprofissional> { profissionalServico });
            return RedirectToAction(nameof(Index), new { idProfissional = profissionalServicoViewModel.IdProfissional });
        }

        // POST: AnamneseController/AlterarExigencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlterarExigencia(ProfissionalServicoViewModel profissionalServicoViewModel)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(
                profissionalServicoViewModel.IdProfissional, profissionalServicoViewModel.IdServico, idEstabelecimento);

            if (profissionalServico != null)
            {
                if (profissionalServicoViewModel.ExigeAnamnese && string.IsNullOrWhiteSpace(profissionalServico.FormAnamnese))
                {
                    return RedirectToAction(nameof(Create), new
                    {
                        idProfissional = profissionalServicoViewModel.IdProfissional,
                        idServico = profissionalServicoViewModel.IdServico
                    });
                }

                profissionalServico.ExigeAnamnese = (sbyte)(profissionalServicoViewModel.ExigeAnamnese ? 1 : 0);
                profissionalService.EditServicoProfissional(
                    profissionalServicoViewModel.IdProfissional,
                    new List<Servicoprofissional> { profissionalServico });
            }
            return RedirectToAction(nameof(Index), new { idProfissional = profissionalServicoViewModel.IdProfissional });
        }

        // POST: AnamneseController/AdicionarServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdicionarServico(uint idProfissional, uint idServico)
        {
            if (idServico > 0)
            {
                var novoVinculo = new Servicoprofissional
                {
                    IdProfissional = idProfissional,
                    IdServico = idServico,
                    IdEstabelecimento = idEstabelecimentoProvisorio,
                    ExigeAnamnese = 0,
                    FormAnamnese = string.Empty,
                    Preco = 0m,
                    PercentualSinal = 0f,
                    QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO",
                    Duracao = 30
                };
                profissionalService.CreateServicoProfissional(novoVinculo);
            }
            return RedirectToAction(nameof(Index), new { idProfissional });
        }

        // POST: AnamneseController/EditarPreco
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPreco(uint idProfissional, uint idServico, decimal preco, uint duracao = 30)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);

            if (profissionalServico != null && preco >= 0)
            {
                profissionalServico.Preco = preco;
                if (duracao > 0)
                {
                    profissionalServico.Duracao = duracao;
                }
                profissionalService.UpdateDadosServicoProfissional(profissionalServico);
            }
            return RedirectToAction(nameof(Index), new { idProfissional });
        }

        // POST: AnamneseController/EditarSinal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarSinal(uint idProfissional, uint idServico, string quandoPagarSinal, float percentualSinal)
        {
            var idEstabelecimento = idEstabelecimentoProvisorio;
            var profissionalServico = profissionalService.GetServicoProfissional(idProfissional, idServico, idEstabelecimento);

            if (profissionalServico != null)
            {
                if (quandoPagarSinal == "NENHUM" || percentualSinal <= 0)
                {
                    profissionalServico.PercentualSinal = 0f;
                    profissionalServico.QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO";
                }
                else
                {
                    profissionalServico.PercentualSinal = Math.Clamp(percentualSinal, 0f, 100f);
                    profissionalServico.QuandoPagarSinal = quandoPagarSinal == "TODO_AGENDAMENTO"
                        ? "TODO_AGENDAMENTO"
                        : "PRIMEIRO_AGENDAMENTO";
                }
                profissionalService.UpdateDadosServicoProfissional(profissionalServico);
            }
            return RedirectToAction(nameof(Index), new { idProfissional });
        }

        // POST: AnamneseController/RemoverServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoverServico(uint idProfissional, uint idServico)
        {
            profissionalService.DeleteServicoProfissional(idProfissional, idServico, idEstabelecimentoProvisorio);
            return RedirectToAction(nameof(Index), new { idProfissional });
        }
    }
}