using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class EstabelecimentoService : IEstabelecimentoService
    {
        private readonly BemMeCareContext context;
        public EstabelecimentoService(BemMeCareContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// Cria um novo estabelecimento no banco de dados e retorna o id do estabelecimento criado.
        /// </summary>
        /// <param name="estabelecimento"> O estabelecimento a ser criado. </param>
        /// <returns> O id do estabelecimento criado. </returns>
        public uint Create(Estabelecimento estabelecimento)
        {
            context.Add(estabelecimento);
            context.SaveChanges();
            return estabelecimento.Id;
        }
        /// <summary>
        /// Exclui um estabelecimento do banco de dados com base no id fornecido. Se o estabelecimento não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="id"> O id do estabelecimento a ser excluído. </param>
        public void Delete(uint id)
        {
            var estabelecimento = context.Estabelecimentos.Find(id);
            if (estabelecimento != null)
            {
                context.Remove(estabelecimento);
                context.SaveChanges();
            }
        }
        /// <summary>
        /// Atualiza as informações de um estabelecimento existente no banco de dados. Se o estabelecimento não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="estabelecimento"> O estabelecimento a ser atualizado. </param>
        public void Edit(Estabelecimento estabelecimento)
        {
            context.Update(estabelecimento);
            context.SaveChanges();
        }
        /// <summary>
        /// Obtém um estabelecimento pelo id.
        /// </summary>
        /// <param name="id">id do estabelecimento</param>
        /// <returns></returns>
        public Estabelecimento? Get(uint id)
        {
            return context.Estabelecimentos.Find(id);
        }
        /// <summary>
        /// Retorna todos os estabelecimentos do banco de dados.
        /// </summary>
        /// <returns>uma coleção de estabelecimentos</returns>
        public IEnumerable<Estabelecimento> GetAll()
        {
            return context.Estabelecimentos.AsNoTracking();
        }
        public IEnumerable<Estabelecimento> GetByNome(string nome)
        {
            return context.Estabelecimentos.Where(e => e.Nome.StartsWith(nome)).AsNoTracking();
        }
        public IEnumerable<Estabelecimento> GetByPessoa(uint idPessoa)
        {
            return context.Estabelecimentos.Where(e => e.IdPessoa == idPessoa).AsNoTracking();
        }
    }
}
