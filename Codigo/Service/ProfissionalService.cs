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

        public void Delete(uint idProfissional)
        {
            var profissional = context.Profissionals.FirstOrDefault(p => p.IdPessoa == idProfissional);
            if (profissional != null)
            {
                context.Remove(profissional);
                context.SaveChanges();
            }
        }

        public void Edit(Profissional profissional)
        {
            var existente = context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                .FirstOrDefault(p => p.IdPessoa == profissional.IdPessoa);

            if (existente != null)
            {
                // Copia os IDs antes de limpar caso seja o mesmo objeto rastreado (ex: nos testes)
                var idsCategorias = profissional.IdCategoria?.Select(c => c.Id).ToList() ?? new List<uint>();
                var idsServicos = profissional.Servicoprofissionals?.Select(sp => sp.IdServico).ToList() ?? new List<uint>();

                context.Entry(existente).CurrentValues.SetValues(profissional);

                // Atualiza categorias vinculadas
                existente.IdCategoria.Clear();
                foreach (var idCat in idsCategorias)
                {
                    var catBanco = context.Categoria.Find(idCat);
                    if (catBanco != null)
                        existente.IdCategoria.Add(catBanco);
                }

                // Remove apenas os serviços que foram desmarcados
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

        public Profissional? Get(uint idProfissional)
        {
            return context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                    .ThenInclude(sp => sp.IdServicoNavigation)
                .FirstOrDefault(p => p.IdPessoa == idProfissional);
        }

        public IEnumerable<Profissional> GetAll()
        {
            return context.Profissionals
                .Include(p => p.IdCategoria)
                .Include(p => p.Servicoprofissionals)
                    .ThenInclude(sp => sp.IdServicoNavigation)
                .AsNoTracking();
        }

        public IEnumerable<Servicoprofissional> GetServicoProfissional(uint idProfissional, uint idEstabelecimento)
        {
            return context.Servicoprofissionals
                .Include(sp => sp.IdServicoNavigation)
                .Where(sp => sp.IdProfissional == idProfissional &&
                             sp.IdEstabelecimento == idEstabelecimento)
                .AsNoTracking()
                .ToList();
        }

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