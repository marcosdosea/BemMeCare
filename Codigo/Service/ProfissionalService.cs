using Core;
using Core.DTO;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class ProfissionalService : IProfissionalService
    {
        private readonly BemMeCareContext context;

        public ProfissionalService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cadastra um novo profissional no estabelecimento vinculando suas categorias e serviços.
        /// </summary>
        /// <param name="profissional">Entidade do profissional a ser cadastrada.</param>
        /// <returns>ID da pessoa associada ao profissional.</returns>
        public uint Create(Profissional profissional)
        {
            if (profissional.IdCategoria != null && profissional.IdCategoria.Any())
            {
                var idCategorias = profissional.IdCategoria.Select(c => c.Id).ToList();
                profissional.IdCategoria.Clear();
                foreach (var id in idCategorias)
                {
                    var cat = context.Categoria.Find(id);
                    if (cat != null)
                    {
                        profissional.IdCategoria.Add(cat);
                    }
                }
            }

            context.Add(profissional);
            context.SaveChanges();
            return profissional.IdPessoa;
        }

        /// <summary>
        /// Remove o vínculo de um profissional pelo seu identificador.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        public void Delete(uint idProfissional)
        {
            var profissional = context.Profissionals.FirstOrDefault(p => p.IdPessoa == idProfissional);
            if (profissional != null)
            {
                context.Remove(profissional);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza os dados cadastrais, categorias e serviços vinculados a um profissional.
        /// </summary>
        /// <param name="profissional">Dados atualizados do profissional.</param>
        public void Edit(Profissional profissional)
        {
            var existente = context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                .FirstOrDefault(p => p.IdPessoa == profissional.IdPessoa);

            if (existente != null)
            {
                var idsCategorias = profissional.IdCategoria?.Select(c => c.Id).ToList() ?? new List<uint>();
                var idsServicos = profissional.Servicoprofissionals?.Select(sp => sp.IdServico).ToList() ?? new List<uint>();

                context.Entry(existente).CurrentValues.SetValues(profissional);

                existente.IdCategoria.Clear();
                foreach (var idCat in idsCategorias)
                {
                    var catBanco = context.Categoria.Find(idCat);
                    if (catBanco != null)
                        existente.IdCategoria.Add(catBanco);
                }

                var servicosParaRemover = existente.Servicoprofissionals
                    .Where(sp => !idsServicos.Contains(sp.IdServico))
                    .ToList();
                foreach (var sp in servicosParaRemover)
                {
                    existente.Servicoprofissionals.Remove(sp);
                }

                var idsExistentes = existente.Servicoprofissionals.Select(sp => sp.IdServico).ToHashSet();
                foreach (var idServ in idsServicos)
                {
                    if (!idsExistentes.Contains(idServ))
                    {
                        existente.Servicoprofissionals.Add(new Servicoprofissional
                        {
                            IdProfissional = existente.IdPessoa,
                            IdEstabelecimento = existente.IdEstabelecimento,
                            IdServico = idServ,
                            ExigeAnamnese = 0,
                            FormAnamnese = string.Empty,
                            Preco = 0m,
                            PercentualSinal = 0f,
                            QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO",
                            Duracao = 0
                        });
                    }
                }

                context.SaveChanges();
            }
            else
            {
                context.Update(profissional);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Obtém um profissional pelo seu identificador incluindo categorias e serviços.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        /// <returns>O profissional encontrado ou nulo.</returns>
        public Profissional? Get(uint idProfissional)
        {
            return context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                    .ThenInclude(sp => sp.IdServicoNavigation)
                .FirstOrDefault(p => p.IdPessoa == idProfissional);
        }

        /// <summary>
        /// Obtém todos os profissionais cadastrados.
        /// </summary>
        /// <returns>Coleção de profissionais.</returns>
        public IEnumerable<Profissional> GetAll()
        {
            return context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                    .ThenInclude(sp => sp.IdServicoNavigation)
                .AsNoTracking();
        }

        /// <summary>
        /// Obtém todos os serviços vinculados a um profissional em um estabelecimento.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        /// <param name="idEstabelecimento">ID do estabelecimento.</param>
        /// <returns>Coleção de serviços vinculados ao profissional.</returns>
        public IEnumerable<Servicoprofissional> GetServicoProfissional(uint idProfissional, uint idEstabelecimento)
        {
            return context.Servicoprofissionals
                .Include(sp => sp.IdServicoNavigation)
                .Where(sp => sp.IdProfissional == idProfissional &&
                             sp.IdEstabelecimento == idEstabelecimento)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// Obtém um serviço específico vinculado a um profissional em um estabelecimento.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        /// <param name="idServico">ID do serviço.</param>
        /// <param name="idEstabelecimento">ID do estabelecimento.</param>
        /// <returns>O vínculo do serviço encontrado ou nulo.</returns>
        public Servicoprofissional? GetServicoProfissional(uint idProfissional, uint idServico, uint idEstabelecimento)
        {
            return context.Servicoprofissionals
                .Include(sp => sp.IdServicoNavigation)
                .AsNoTracking()
                .FirstOrDefault(sp =>
                    sp.IdProfissional == idProfissional &&
                    sp.IdServico == idServico &&
                    sp.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Atualiza as configurações de anamnese dos serviços vinculados a um profissional.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        /// <param name="servicos">Lista de serviços com dados de anamnese atualizados.</param>
        public void EditServicoProfissional(uint idProfissional, IEnumerable<Servicoprofissional> servicos)
        {
            foreach (var servico in servicos)
            {
                var existente = context.Servicoprofissionals.FirstOrDefault(sp =>
                    sp.IdProfissional == idProfissional &&
                    sp.IdEstabelecimento == servico.IdEstabelecimento &&
                    sp.IdServico == servico.IdServico);

                if (existente == null)
                    continue;

                if (servico.ExigeAnamnese == 1 && string.IsNullOrWhiteSpace(servico.FormAnamnese))
                    continue;

                existente.ExigeAnamnese = servico.ExigeAnamnese;
                existente.FormAnamnese = servico.FormAnamnese ?? string.Empty;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Vincula um novo serviço a um profissional no estabelecimento.
        /// </summary>
        /// <param name="servicoProfissional">Vínculo de serviço a ser adicionado.</param>
        public void CreateServicoProfissional(Servicoprofissional servicoProfissional)
        {
            var jaExiste = context.Servicoprofissionals.Any(sp =>
                sp.IdProfissional == servicoProfissional.IdProfissional &&
                sp.IdEstabelecimento == servicoProfissional.IdEstabelecimento &&
                sp.IdServico == servicoProfissional.IdServico);

            if (jaExiste)
                return;

            servicoProfissional.FormAnamnese ??= string.Empty;
            if (string.IsNullOrWhiteSpace(servicoProfissional.QuandoPagarSinal))
            {
                servicoProfissional.QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO";
            }

            context.Servicoprofissionals.Add(servicoProfissional);
            context.SaveChanges();
        }

        /// <summary>
        /// Atualiza os dados comerciais (preço, percentual de sinal, regra de pagamento do sinal e duração)
        /// de um serviço vinculado ao profissional.
        /// </summary>
        /// <param name="servicoProfissional">Dados atualizados do serviço do profissional.</param>
        public void UpdateDadosServicoProfissional(Servicoprofissional servicoProfissional)
        {
            var existente = context.Servicoprofissionals.FirstOrDefault(sp =>
                sp.IdProfissional == servicoProfissional.IdProfissional &&
                sp.IdEstabelecimento == servicoProfissional.IdEstabelecimento &&
                sp.IdServico == servicoProfissional.IdServico);

            if (existente == null)
                return;

            if (servicoProfissional.Preco >= 0)
            {
                existente.Preco = servicoProfissional.Preco;
            }

            if (servicoProfissional.PercentualSinal is >= 0 and <= 100)
            {
                existente.PercentualSinal = servicoProfissional.PercentualSinal;
            }

            if (!string.IsNullOrWhiteSpace(servicoProfissional.QuandoPagarSinal))
            {
                existente.QuandoPagarSinal = servicoProfissional.QuandoPagarSinal;
            }

            if (servicoProfissional.Duracao > 0)
            {
                existente.Duracao = servicoProfissional.Duracao;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Remove o vínculo de um serviço com o profissional no estabelecimento.
        /// </summary>
        /// <param name="idProfissional">ID do profissional.</param>
        /// <param name="idServico">ID do serviço.</param>
        /// <param name="idEstabelecimento">ID do estabelecimento.</param>
        public void DeleteServicoProfissional(uint idProfissional, uint idServico, uint idEstabelecimento)
        {
            var existente = context.Servicoprofissionals.FirstOrDefault(sp =>
                sp.IdProfissional == idProfissional &&
                sp.IdServico == idServico &&
                sp.IdEstabelecimento == idEstabelecimento);

            if (existente != null)
            {
                context.Servicoprofissionals.Remove(existente);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Obtém informações dos profissionais de um estabelecimento e seus respectivos serviços.
        /// </summary>
        /// <param name="idEstabelecimento">id do estabelecimento</param>
        /// <returns>Coleção com informações dos profissionais e seus serviços.</returns>
        public IEnumerable<ProfissionalServicosDTO> GetServicosProfissionais(uint idEstabelecimento)
        {
            return context.Profissionals
                .AsNoTracking()
                .Where(p => p.EstaAtivo == 1
                        && p.IdEstabelecimento == idEstabelecimento
                        && p.Servicoprofissionals.Any(sp => sp.IdEstabelecimento == idEstabelecimento))
                .Select(p => new ProfissionalServicosDTO
                {
                    Id = p.IdPessoa,
                    Nome = p.IdPessoaNavigation.Nome,
                    Servicos = p.Servicoprofissionals
                        .Where(sp => sp.IdEstabelecimento == idEstabelecimento)
                        .Select(sp => new ServicoProfissionalDTO
                        {
                            Id = sp.IdServico,
                            Nome = sp.IdServicoNavigation.Nome,
                            DuracaoEmMinutos = sp.Duracao,
                            Preco = sp.Preco
                        }).ToList()
                })
                .OrderBy(p => p.Nome)
                .ToList();
        }
    }
}