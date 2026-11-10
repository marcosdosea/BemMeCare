using BemMeCareWeb.Mappers;
using BemMeCareWeb.Models;
using Core;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Controllers
{
    public class ProfissionalController : Controller
    {
        // ID provisório que depois será obtido automaticamente pelo usuário logado no sistema
        private const uint idEstabelecimentoProvisorio = 1;

        private readonly IProfissionalService profissionalService;
        private readonly ICategoriaService categoriaService;
        private readonly IServicoService servicoService;
        private readonly IPessoaService pessoaService;

        public ProfissionalController(
            IProfissionalService profissionalService,
            ICategoriaService categoriaService,
            IServicoService servicoService,
            IPessoaService pessoaService)
        {
            this.profissionalService = profissionalService;
            this.categoriaService = categoriaService;
            this.servicoService = servicoService;
            this.pessoaService = pessoaService;
        }

        // GET: ProfissionalController
        public ActionResult Index()
        {
            var listaProfissionais = profissionalService.GetAll();
            var listaProfissionaisViewModel = listaProfissionais.ToViewModel().ToList();

            // Garante que o Nome da pessoa seja carregado na listagem
            var pessoas = pessoaService.GetAll().ToDictionary(p => p.Id, p => p.Nome);
            foreach (var item in listaProfissionaisViewModel)
            {
                if (string.IsNullOrEmpty(item.Nome) && pessoas.TryGetValue(item.IdPessoa, out var nomePessoa))
                {
                    item.Nome = nomePessoa;
                }
            }

            return View(listaProfissionaisViewModel);
        }

        // GET: ProfissionalController/Details/5
        public ActionResult Details(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional?.ToViewModel() ?? new ProfissionalViewModel();

            if (profissional != null && string.IsNullOrEmpty(profissionalViewModel.Nome))
            {
                var pessoa = pessoaService.Get(id);
                if (pessoa != null)
                {
                    profissionalViewModel.Nome = pessoa.Nome;
                    profissionalViewModel.Cpf = pessoa.Cpf;
                }
            }

            return View(profissionalViewModel);
        }

        // GET: ProfissionalController/Create
        public ActionResult Create()
        {
            var profissionalViewModel = new ProfissionalViewModel
            {
                GerirHorario = 0,
                EstaAtivo = 1,
                GerirServico = 0,
                ReceberPagamentoDireto = 0,
                DuracaoIntervaloTempo = 0,
                EProfissional = 1,
                EAtendente = 0
            };
            CarregarListas(profissionalViewModel);
            return View(profissionalViewModel);
        }

        // POST: ProfissionalController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfissionalViewModel profissionalViewModel)
        {
            if (ModelState.IsValid)
            {
                profissionalViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;

                if (profissionalViewModel.IdPessoa == 0 && !string.IsNullOrWhiteSpace(profissionalViewModel.Cpf))
                {
                    var pessoa = BuscarPessoaPorCpf(profissionalViewModel.Cpf);
                    if (pessoa == null)
                    {
                        ModelState.AddModelError(nameof(ProfissionalViewModel.Cpf), "CPF não encontrado no sistema. Cadastre a Pessoa primeiro.");
                        CarregarListas(profissionalViewModel);
                        return View(profissionalViewModel);
                    }
                    profissionalViewModel.IdPessoa = pessoa.Id;
                }

                var profissional = profissionalViewModel.ToEntity();
                profissionalService.Create(profissional);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProfissionalController/Edit/5
        public ActionResult Edit(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional?.ToViewModel() ?? new ProfissionalViewModel();
            CarregarListas(profissionalViewModel);
            return View(profissionalViewModel);
        }

        // POST: ProfissionalController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfissionalViewModel profissionalViewModel)
        {
            if (ModelState.IsValid)
            {
                profissionalViewModel.IdEstabelecimento = idEstabelecimentoProvisorio;
                var profissional = profissionalViewModel.ToEntity();
                profissionalService.Edit(profissional);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProfissionalController/Delete/5
        public ActionResult Delete(uint id)
        {
            var profissional = profissionalService.Get(id);
            var profissionalViewModel = profissional?.ToViewModel() ?? new ProfissionalViewModel();
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

        private void CarregarListas(ProfissionalViewModel profissionalViewModel)
        {
            var categorias = categoriaService.GetAll(idEstabelecimentoProvisorio);
            var servicos = servicoService.GetAll()
                .Where(servico => servico.IdEstabelecimento == idEstabelecimentoProvisorio);

            profissionalViewModel.ListaCategorias = new SelectList(
                categorias, nameof(Categorium.Id), nameof(Categorium.Nome), profissionalViewModel.IdCategoriaSelecionada);
            profissionalViewModel.ServicosDisponiveis = servicos;
        }

        private Pessoa? BuscarPessoaPorCpf(string cpf)
        {
            var cpfSomenteNumeros = new string(cpf.Where(char.IsDigit).ToArray());
            return pessoaService.GetAll()
                .FirstOrDefault(pessoa => new string(pessoa.Cpf.Where(char.IsDigit).ToArray()) == cpfSomenteNumeros);
        }
    }
}