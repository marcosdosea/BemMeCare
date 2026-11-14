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
        /// Lança InvalidOperationException caso haja sobreposição de horários para o mesmo profissional.
        /// </summary>
        /// <param name="horario"> O horário a ser criado. </param>
        /// <returns> O id do horário criado. </returns>
        public uint Create(Horario horario)
        {
            if (TemSobreposicao(horario))
            {
                throw new InvalidOperationException("Já existe um horário conflitante para este profissional no mesmo dia da semana.");
            }

            context.Add(horario);
            context.SaveChanges();
            return horario.Id;
        }

        /// <summary>
        /// Exclui um horário do banco de dados com base no id e no estabelecimento fornecidos.
        /// Se o horário não for encontrado ou pertencer a outro estabelecimento, nenhuma ação será realizada.
        /// </summary>
        /// <param name="id"> O id do horário a ser excluído. </param>
        /// <param name="idEstabelecimento"> O id do estabelecimento associado. </param>
        public void Delete(uint id, uint idEstabelecimento)
        {
            var horario = context.Horarios.FirstOrDefault(
                h => h.Id == id && h.IdEstabelecimento == idEstabelecimento);

            if (horario != null)
            {
                context.Remove(horario);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Atualiza as informações de um horário existente no estabelecimento informado.
        /// Se o horário não for encontrado no estabelecimento, nenhuma ação será realizada.
        /// Lança InvalidOperationException se a atualização causar choque de horários.
        /// </summary>
        /// <param name="horario"> O horário a ser atualizado. </param>
        /// <param name="idEstabelecimento"> O id do estabelecimento associado. </param>
        public void Edit(Horario horario, uint idEstabelecimento)
        {
            var horarioExistente = context.Horarios.FirstOrDefault(
                h => h.Id == horario.Id && h.IdEstabelecimento == idEstabelecimento);

            if (horarioExistente != null)
            {
                if (TemSobreposicao(horario, horario.Id))
                {
                    throw new InvalidOperationException("Já existe um horário conflitante para este profissional no mesmo dia da semana.");
                }

                horarioExistente.InicioTurno = horario.InicioTurno;
                horarioExistente.FimTurno = horario.FimTurno;
                horarioExistente.InicioVigencia = horario.InicioVigencia;
                horarioExistente.FimVigencia = horario.FimVigencia;
                horarioExistente.DiaSemana = horario.DiaSemana;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Obtém um horário pelo id, limitado ao estabelecimento informado.
        /// </summary>
        /// <param name="id">id do horário</param>
        /// <param name="idEstabelecimento">id do estabelecimento</param>
        /// <returns>O horário correspondente ou nulo se não encontrado.</returns>
        public Horario? Get(uint id, uint idEstabelecimento)
        {
            return context.Horarios
                .AsNoTracking()
                .FirstOrDefault(h => h.Id == id && h.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Recupera todos os horários pertencentes ao estabelecimento informado.
        /// </summary>
        /// <param name="idEstabelecimento">id do estabelecimento</param>
        /// <returns>uma coleção de horários do estabelecimento</returns>
        public IEnumerable<Horario> GetAll(uint idEstabelecimento)
        {
            return context.Horarios
                .AsNoTracking()
                .Where(h => h.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Pega os horários de um profissional específico dentro de um determinado estabelecimento.
        /// </summary>
        /// <param name="idProfissional">id do profissional</param>
        /// <param name="idEstabelecimento">id do estabelecimento</param>
        /// <returns>coleção de horários do profissional no estabelecimento</returns>
        public IEnumerable<Horario> GetByProfissional(uint idProfissional, uint idEstabelecimento)
        {
            return context.Horarios
                .AsNoTracking()
                .Where(h => h.IdProfissional == idProfissional && h.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Pega os horários de um profissional específico em um determinado dia da semana e estabelecimento.
        /// </summary>
        /// <param name="diaSemana">dia da semana</param>
        /// <param name="idProfissional">id do profissional</param>
        /// <param name="idEstabelecimento">id do estabelecimento</param>
        /// <returns>coleção de horários correspondentes</returns>
        public IEnumerable<Horario> GetByDiaSemana(string diaSemana, uint idProfissional, uint idEstabelecimento)
        {
            return context.Horarios
                .AsNoTracking()
                .Where(h => h.DiaSemana == diaSemana &&
                            h.IdProfissional == idProfissional &&
                            h.IdEstabelecimento == idEstabelecimento);
        }

        /// <summary>
        /// Verifica se há sobreposição/choque de turno e vigência para o profissional no mesmo dia da semana,
        /// independentemente de ser no mesmo ou em outro estabelecimento.
        /// </summary>
        private bool TemSobreposicao(Horario novoHorario, uint? idIgnorar = null)
        {
            var horariosExistentes = context.Horarios
                .AsNoTracking()
                .Where(h => h.IdProfissional == novoHorario.IdProfissional &&
                            h.DiaSemana == novoHorario.DiaSemana &&
                            (!idIgnorar.HasValue || h.Id != idIgnorar.Value))
                .ToList();

            foreach (var existente in horariosExistentes)
            {
                // Choque de turno diário: [InicioTurno, FimTurno)
                bool sobrepoeTurno = novoHorario.InicioTurno < existente.FimTurno &&
                                     novoHorario.FimTurno > existente.InicioTurno;

                if (!sobrepoeTurno)
                {
                    continue;
                }

                // Choque de vigência: [InicioVigencia, FimVigencia ?? MaxValue]
                var novoFim = novoHorario.FimVigencia ?? DateTime.MaxValue;
                var existenteFim = existente.FimVigencia ?? DateTime.MaxValue;

                bool sobrepoeVigencia = novoHorario.InicioVigencia <= existenteFim &&
                                        novoFim >= existente.InicioVigencia;

                if (sobrepoeVigencia)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
