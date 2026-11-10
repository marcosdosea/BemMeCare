using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class ServicoService : IServicoService
    {
        private readonly BemMeCareContext context;

        public ServicoService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cria um novo serviço no banco de dados e retorna o id do serviço criado.
        /// </summary>
        /// <param name="servico">O serviço a ser criado.</param>
        /// <returns>O id do serviço criado.</returns>
        public uint Create(Servico servico)
        {
            context.Add(servico);
            context.SaveChanges();
            return servico.Id;
        }

        /// <summary>
        /// Exclui um serviço pelo id. Se ele não for encontrado, nenhuma ação é realizada.
        /// </summary>
        /// <param name="id">O id do serviço a ser excluído.</param>
        public void Delete(uint id)
        {
            var servico = context.Servicos.Find(id);
            if (servico != null)
            {
                context.Remove(servico);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de um serviço existente.
        /// </summary>
        /// <param name="servico">O serviço com os dados atualizados.</param>
        public void Edit(Servico servico)
        {
            context.Update(servico);
            context.SaveChanges();
        }

        /// <summary>
        /// Obtém um serviço pelo id.
        /// </summary>
        /// <param name="id">O id do serviço.</param>
        /// <returns>O serviço encontrado ou null, se não existir.</returns>
        public Servico? Get(uint id)
        {
            return context.Servicos.Find(id);
        }

        /// <summary>
        /// Recupera todos os serviços do banco de dados.
        /// </summary>
        /// <returns>Uma coleção de serviços.</returns>
        public IEnumerable<Servico> GetAll()
        {
            return context.Servicos.AsNoTracking();
        }

        /// <summary>
        /// Recupera serviços cujo nome começa com o texto informado.
        /// </summary>
        /// <param name="nome">O início do nome a pesquisar.</param>
        /// <returns>Uma coleção dos serviços correspondentes.</returns>
        public IEnumerable<Servico> GetByNome(string nome)
        {
            return context.Servicos
                .Where(servico => servico.Nome.StartsWith(nome))
                .AsNoTracking();
        }
    }
}