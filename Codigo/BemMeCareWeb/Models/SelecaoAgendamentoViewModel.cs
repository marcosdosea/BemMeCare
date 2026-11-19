using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class SelecaoAgendamentoViewModel
{
    public uint IdProfissional {  get; set; }

    public IEnumerable<uint> IdServicosSelecionados { get; set; } = new List<uint>();
}
