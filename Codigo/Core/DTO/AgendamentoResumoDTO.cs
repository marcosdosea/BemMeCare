using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class AgendamentoResumoDTO
    {
        public string NomeProfissional { get; set; } = string.Empty;
        public IEnumerable<ServicoProfissionalDTO> Servicos { get; set; } = [];
        public decimal PrecoTotal { get; set; }
        public uint DuracaoTotalEmMinutos { get; set; }
        public TimeOnly HorarioAtendimento { get; set; }
    }
}
