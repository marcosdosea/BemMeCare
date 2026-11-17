namespace Core.DTO
{
    public class DashboardItemDTO
    {
        public string Label { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }

    public class DashboardResumoServicosDTO
    {
        public string ServicoMaisUsadoNome { get; set; } = "-";
        public int ServicoMaisUsadoQuantidade { get; set; }

        public string CategoriaMaisUsadaNome { get; set; } = "-";
        public int CategoriaMaisUsadaQuantidade { get; set; }

        public string ServicoMenosUsadoNome { get; set; } = "-";
        public int ServicoMenosUsadoQuantidade { get; set; }
    }

    public class DashboardResumoFaturamentoDTO
    {
        public decimal FaturamentoMesAtual { get; set; }
        public decimal PercentualVariacaoMensal { get; set; }

        public decimal FaturamentoSemanal { get; set; }
        public decimal PercentualVariacaoSemanal { get; set; }

        public string ServicoMaisFaturaNome { get; set; } = "-";
        public decimal ServicoMaisFaturaValor { get; set; }

        public string ServicoMenosFaturaNome { get; set; } = "-";
        public decimal ServicoMenosFaturaValor { get; set; }

        public IEnumerable<DashboardItemDTO> FaturamentoUltimos12Meses { get; set; } = new List<DashboardItemDTO>();
    }
}