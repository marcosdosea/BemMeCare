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

        /// <summary>
        /// Cria um novo profissional no banco de dados e retorna o id do profissional criado.
        /// </summary>
        /// <param name="profissional">O profissional a ser criado.</param>
        /// <returns>O id (IdPessoa) do profissional criado.</returns>
        public uint Create(Profissional profissional)
        {
            context.Add(profissional);
            context.SaveChanges();
            return profissional.IdPessoa;
        }

        /// <summary>
        /// Exclui um profissional do banco de dados com base no id fornecido. Se o profissional não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="idProfissional">O id (IdPessoa) do profissional a ser excluído.</param>
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
        /// Atualiza as informações de um profissional existente no banco de dados.
        /// </summary>
        /// <param name="profissional">O profissional a ser atualizado.</param>
        public void Edit(Profissional profissional)
        {
            context.Update(profissional);
            context.SaveChanges();
        }

        /// <summary>
        /// Obtém um profissional pelo id (IdPessoa).
        /// </summary>
        /// <param name="idProfissional">Id do profissional.</param>
        /// <returns>O profissional encontrado ou null se não for encontrado.</returns>
        public Profissional? Get(uint idProfissional)
        {
            return context.Profissionals.FirstOrDefault(p => p.IdPessoa == idProfissional);
        }

        /// <summary>
        /// Recupera todos os profissionais do banco de dados.
        /// </summary>
        /// <returns>Uma coleção de profissionais.</returns>
        public IEnumerable<Profissional> GetAll()
        {
            return context.Profissionals.AsNoTracking();
        }
    }
}