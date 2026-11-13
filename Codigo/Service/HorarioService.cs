using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class HorarioService : IHorarioService
    {
        private readonly BemMeCareContext context;

        public HorarioService(BemMeCareContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Cria um novo horário no banco de dados e retorna o id do horário criado.
        /// </summary>
        /// <param name="horario"> O horário a ser criado. </param>
        /// <returns> O id do horário criado. </returns>

        public uint Create(Horario horario)
        {
            context.Add(horario);
            context.SaveChanges();
            return horario.Id;
        }

        /// <summary>
        /// Exclui um horário do banco de dados com base no id fornecido. Se o horário não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="id"> O id do horário a ser excluído. </param>
        public void Delete(uint id)
        {
            var horario = context.Horarios.Find(id);
            if (horario != null)
            {
                context.Remove(horario);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de um horário existente no banco de dados. Se o horário não for encontrado, nenhuma ação será realizada.
        /// </summary>
        /// <param name="horario"> O horário a ser atualizado. </param>

        public void Edit(Horario horario)
        {
            context.Update(horario);
            context.SaveChanges();
        }

        /// <summary>
        /// Obtém um horário pelo id.
        /// </summary>
        /// <param name="id">id do horário</param>
        /// <returns></returns>
        public Horario? Get(uint id)
        {
            return context.Horarios.Find(id);
        }

        /// <summary>
        /// Recupera todos os horários do banco de dados.
        /// </summary>
        /// <returns>uma coleção de horários</returns>
        public IEnumerable<Horario> GetAll()
        {
            return context.Horarios.AsNoTracking();
        }

        /// <summary>
        /// Pega o horário de um profissional específico com base no id do profissional.
        /// </summary>
        /// <param name="idProfissional"></param>
        /// <returns></returns>
        public IEnumerable<Horario> GetByProfissional(uint idProfissional)
        {
            return context.Horarios
                .AsNoTracking()
                .Where(h => h.IdProfissional == idProfissional);
        }

        /// <summary>
        /// Pega os horários de um profissional específico em um determinado dia da semana.
        /// </summary>
        /// <param name="diaSemana"></param>
        /// <param name="idProfissional"></param>
        /// <returns></returns>

        public IEnumerable<Horario> GetByDiaSemana(string diaSemana, uint idProfissional)
        {
            return context.Horarios
                .AsNoTracking()
                .Where(h => h.DiaSemana == diaSemana && h.IdProfissional == idProfissional);
        }
    }
}
