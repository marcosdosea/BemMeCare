
using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class CategoriaService : ICategoriaService
    {
        private readonly BemMeCareContext context;

        public CategoriaService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cria uma nova categoria no banco de dados e retorna o id da categoria criada.
        /// </summary>
        /// <param name="categoria">A categoria a ser criado.</param>
        /// <returns>O id da categoria criada.</returns>
        public uint Create(Categorium categoria)
        {
            context.Add(categoria);
            context.SaveChanges();
            return categoria.Id;
        }

        /// <summary>
        /// Exclui uma categoria do banco de dados com base no id fornecido. Se a categoria não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="id">O id da categoria a ser excluído.</param>
        public void Delete(uint id, uint idEstabelecimento)
        {
            var categoria = context.Categoria.Find(id);
            if (categoria != null)
            {
                context.Remove(categoria);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de uma categoria existente no banco de dados. Se a categoria não for encontrado, nenhuma ação será realizada.    
        /// </summary>
        /// <param name="categoria">A categoria a ser atualizado.</param>
        public void Edit(Categorium categoria)
        {
            context.Update(categoria);
            context.SaveChanges();
        }

        /// <summary>
        /// Obtém uma categoria pelo id.
        /// </summary>
        /// <param name="id">id da categoria</param>
        /// <returns>Uma categoria ou null, se não existir.</returns>
        public Categorium? Get(uint id, uint idEstabelecimento)
        {
            return context.Categoria.Find(id);
        }

        /// <summary>
        /// Recupera todos as categorias do banco de dados.
        /// </summary>
        /// <returns>Uma coleção de categorias</returns>
        public IEnumerable<Categorium> GetAll(uint idEstabelecimento)
        {
            return context.Categoria.AsNoTracking();
        }
    }
}
