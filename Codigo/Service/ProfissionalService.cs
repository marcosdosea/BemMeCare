using Core;
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
                context.Entry(existente).CurrentValues.SetValues(profissional);

                existente.IdCategoria.Clear();
                if (profissional.IdCategoria != null)
                {
                    foreach (var cat in profissional.IdCategoria)
                    {
                        var catBanco = context.Categoria.Find(cat.Id);
                        if (catBanco != null)
                            existente.IdCategoria.Add(catBanco);
                    }
                }

                existente.Servicoprofissionals.Clear();
                if (profissional.Servicoprofissionals != null)
                {
                    foreach (var sp in profissional.Servicoprofissionals)
                    {
                        existente.Servicoprofissionals.Add(new Servicoprofissional
                        {
                            IdProfissional = existente.IdPessoa,
                            IdEstabelecimento = existente.IdEstabelecimento,
                            IdServico = sp.IdServico
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
    }
}