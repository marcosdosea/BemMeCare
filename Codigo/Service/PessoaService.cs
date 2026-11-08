using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class PessoaService : IPessoaService
    {
        private readonly BemMeCareContext context;

        public PessoaService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cria uma nova pessoa no banco de dados e retorna o id da pessoa criada.
        /// </summary>
        /// <param name="pessoa"> A pessoa a ser criada. </param>
        /// <returns> O id da pessoa criada. </returns>
        public uint Create(Pessoa pessoa)
        {
            context.Add(pessoa);
            context.SaveChanges();
            return pessoa.Id;
        }

        /// <summary>
        /// Exclui uma pessoa do banco de dados com base no id fornecido. Se a pessoa não for encontrada, nenhuma ação será realizada.
        /// </summary>
        /// <param name="id"> O id da pessoa a ser excluída. </param>
        public void Delete(uint id)
        {
            var pessoa = context.Pessoas.Find(id);
            if (pessoa != null)
            {
                context.Remove(pessoa);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de uma pessoa existente no banco de dados. Se a pessoa não for encontrada, nenhuma ação será realizada.
        /// </summary>
        /// <param name="pessoa"> A pessoa a ser atualizada. </param>
        public void Edit(Pessoa pessoa)
        {
            context.Update(pessoa);
            context.SaveChanges();
        }

        /// <summary>
        /// Obtém uma pessoa pelo id.
        /// </summary>
        /// <param name="id">id da pessoa</param>
        /// <returns></returns>
        public Pessoa? Get(uint id)
        {
            return context.Pessoas.Find(id);
        }

        /// <summary>
        /// Recupera todos as pessoas do banco de dados.
        /// </summary>
        /// <returns>uma coleção de pessoas</returns>
        public IEnumerable<Pessoa> GetAll()
        {
            return context.Pessoas.AsNoTracking();
        }

        public IEnumerable<Pessoa> GetByNome(string nome)
        {
            return context.Pessoas.Where(p => p.Nome.StartsWith(nome)).AsNoTracking();
        }
    }
}
