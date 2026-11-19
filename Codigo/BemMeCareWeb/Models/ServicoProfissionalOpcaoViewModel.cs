namespace BemMeCareWeb.Models;

public class ServicoProfissionalOpcaoViewModel
{
    public uint IdServico { get; set; }

    public uint IdProfissional { get; set; }

    public string NomeServico { get; set; } = string.Empty;

    public string NomeProfissional { get; set; } = string.Empty;
}
