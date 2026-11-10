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
        /// <param name="servico">Os dados do serviço a ser criado.</param>
        /// <param name="idEstabelecimento">O estabelecimento proprietário do serviço.</param>
        /// <returns>O id do serviço criado.</returns>
        public uint Create(Servico servico, uint idEstabelecimento)
        {
            servico.IdEstabelecimento = idEstabelecimento;
            context.Add(servico);
            context.SaveChanges();
            return servico.Id;
        }

        /// <summary>
        /// Exclui um serviço do estabelecimento informado. Se não for encontrado, nenhuma ação é realizada.
        /// </summary>
        /// <param name="id">O id do serviço a ser excluído.</param>
        /// <param name="idEstabelecimento">O estabelecimento proprietário do serviço.</param>
        public void Delete(uint id, uint idEstabelecimento)
        {
            var servico = context.Servicos.FirstOrDefault(
                item => item.Id == id && item.IdEstabelecimento == idEstabelecimento);

            if (servico != null)
            {
                context.Remove(servico);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de um serviço pertencente ao estabelecimento informado.
        /// </summary>
        /// <param name="servico">O serviço com os dados atualizados.</param>
        /// <param name="idEstabelecimento">O estabelecimento proprietário do serviço.</param>
        public void Edit(Servico servico, uint idEstabelecimento)
        {
            var servicoExistente = context.Servicos.FirstOrDefault(
                item => item.Id == servico.Id && item.IdEstabelecimento == idEstabelecimento);

            if (servicoExistente != null)
            {
                servicoExistente.Nome = servico.Nome;
                servicoExistente.IdCategoria = servico.IdCategoria;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Obtém um serviço pelo id, limitado ao estabelecimento informado.
        /// </summary>
        /// <param name="id">O id do serviço.</param>
        /// <param name="idEstabelecimento">O estabelecimento proprietário do serviço.</param>
        /// <returns>O serviço encontrado ou null, se não existir.</returns>
        public Servico? Get(uint id, uint idEstabelecimento)
        {
            return context.Servicos
                .AsNoTracking()
                .FirstOrDefault(
                    servico => servico.Id == id && servico.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Recupera os serviços pertencentes ao estabelecimento informado.
        /// </summary>
        /// <param name="idEstabelecimento">O estabelecimento proprietário dos serviços.</param>
        /// <returns>Uma coleção de serviços.</returns>
        public IEnumerable<Servico> GetAll(uint idEstabelecimento)
        {
            return context.Servicos
                .Where(servico => servico.IdEstabelecimento == idEstabelecimento)
                .AsNoTracking();
        }

        /// <summary>
        /// Recupera serviços do estabelecimento informado cujo nome começa com o texto pesquisado.
        /// </summary>
        /// <param name="nome">O início do nome a pesquisar.</param>
        /// <param name="idEstabelecimento">O estabelecimento proprietário dos serviços.</param>
        /// <returns>Uma coleção dos serviços correspondentes.</returns>
        public IEnumerable<Servico> GetByNome(string nome, uint idEstabelecimento)
        {
            return context.Servicos
                .Where(servico =>
                    servico.IdEstabelecimento == idEstabelecimento &&
                    servico.Nome.StartsWith(nome))
                .AsNoTracking();
        }
    }
}