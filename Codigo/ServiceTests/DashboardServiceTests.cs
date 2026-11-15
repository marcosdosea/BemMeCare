using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;

namespace Service.Tests
{
    [TestClass]
    public class DashboardServiceTests
    {
        private BemMeCareContext contexto = null!;
        private IDashboardService dashboardService = null!;

        [TestInitialize]
        public void Inicializar()
        {
            var nomeBanco = $"BemMeCare_Dashboard_{Guid.NewGuid()}";

            var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
                .UseInMemoryDatabase(nomeBanco)
                .Options;

            contexto = new BemMeCareContext(opcoes);

            contexto.Pessoas.AddRange(
                new Pessoa
                {
                    Id = 1,
                    Nome = "Fulano de Tal",
                    Email = "fulano@teste.com",
                    Senha = "123",
                    Cpf = "11111111111",
                    Telefone = "79999990001",
                    DataDeNascimento = new DateTime(1990, 1, 1),
                    Cor = "Parda",
                    Genero = "Homem",
                    Sexualidade = "Heterossexual",
                    Cep = "49000000",
                    Rua = "Rua A",
                    Bairro = "Centro",
                    Numero = "10",
                    Complemento = "Casa",
                    Cidade = "Aracaju",
                    Estado = "SE"
                },
                new Pessoa
                {
                    Id = 2,
                    Nome = "Maria Cliente",
                    Email = "maria@teste.com",
                    Senha = "123",
                    Cpf = "22222222222",
                    Telefone = "79999990002",
                    DataDeNascimento = new DateTime(1995, 5, 10),
                    Cor = "Branca",
                    Genero = "Mulher",
                    Sexualidade = "Heterossexual",
                    Cep = "49000000",
                    Rua = "Rua B",
                    Bairro = "Centro",
                    Numero = "20",
                    Complemento = "Apto",
                    Cidade = "Aracaju",
                    Estado = "SE"
                },
                new Pessoa
                {
                    Id = 3,
                    Nome = "João Cliente",
                    Email = "joao@teste.com",
                    Senha = "123",
                    Cpf = "33333333333",
                    Telefone = "79999990003",
                    DataDeNascimento = new DateTime(1998, 8, 20),
                    Cor = "Preta",
                    Genero = "Homem",
                    Sexualidade = "Bissexual",
                    Cep = "49000000",
                    Rua = "Rua C",
                    Bairro = "Centro",
                    Numero = "30",
                    Complemento = "Casa",
                    Cidade = "Aracaju",
                    Estado = "SE"
                });

            contexto.Categoria.AddRange(
                new Categorium { Id = 1, IdEstabelecimento = 1, Nome = "Cabelo" },
                new Categorium { Id = 2, IdEstabelecimento = 1, Nome = "Estética" },
                new Categorium { Id = 3, IdEstabelecimento = 2, Nome = "Outra Clinica" });

            contexto.Servicos.AddRange(
                new Servico { Id = 1, IdCategoria = 1, IdEstabelecimento = 1, Nome = "Corte" },
                new Servico { Id = 2, IdCategoria = 2, IdEstabelecimento = 1, Nome = "Depilação" },
                new Servico { Id = 3, IdCategoria = 3, IdEstabelecimento = 2, Nome = "Serviço Externo" });

            contexto.Agendamentos.AddRange(
                new Agendamento
                {
                    Id = 1,
                    IdEstabelecimento = 1,
                    IdProfissional = 1,
                    IdPessoaCliente = 2,
                    IdPessoaMarcou = 2,
                    DataHoraAtendimento = new DateTime(2026, 5, 10, 10, 0, 0),
                    DataHoraFimAtendimento = new DateTime(2026, 5, 10, 11, 0, 0),
                    DataHoraCriacao = new DateTime(2026, 5, 1, 9, 0, 0),
                    DuracaoMinutos = 60,
                    PrecoFinal = 100m,
                    ValorPagoSinal = 20m,
                    StatusAgendamento = "CO",
                    StatusPagamento = "FI"
                },
                new Agendamento
                {
                    Id = 2,
                    IdEstabelecimento = 1,
                    IdProfissional = 1,
                    IdPessoaCliente = 2,
                    IdPessoaMarcou = 2,
                    DataHoraAtendimento = new DateTime(2026, 5, 15, 14, 0, 0),
                    DataHoraFimAtendimento = new DateTime(2026, 5, 15, 15, 0, 0),
                    DataHoraCriacao = new DateTime(2026, 5, 2, 9, 0, 0),
                    DuracaoMinutos = 60,
                    PrecoFinal = 100m,
                    ValorPagoSinal = 20m,
                    StatusAgendamento = "CO",
                    StatusPagamento = "FI"
                },
                new Agendamento
                {
                    Id = 3,
                    IdEstabelecimento = 1,
                    IdProfissional = 1,
                    IdPessoaCliente = 3,
                    IdPessoaMarcou = 3,
                    DataHoraAtendimento = new DateTime(2026, 5, 20, 16, 0, 0),
                    DataHoraFimAtendimento = new DateTime(2026, 5, 20, 17, 0, 0),
                    DataHoraCriacao = new DateTime(2026, 5, 3, 9, 0, 0),
                    DuracaoMinutos = 60,
                    PrecoFinal = 50m,
                    ValorPagoSinal = 10m,
                    StatusAgendamento = "CO",
                    StatusPagamento = "FI"
                },
                new Agendamento
                {
                    Id = 4,
                    IdEstabelecimento = 1,
                    IdProfissional = 1,
                    IdPessoaCliente = 3,
                    IdPessoaMarcou = 3,
                    DataHoraAtendimento = new DateTime(2026, 5, 22, 16, 0, 0),
                    DataHoraFimAtendimento = new DateTime(2026, 5, 22, 17, 0, 0),
                    DataHoraCriacao = new DateTime(2026, 5, 4, 9, 0, 0),
                    DuracaoMinutos = 60,
                    PrecoFinal = 500m,
                    ValorPagoSinal = 0m,
                    StatusAgendamento = "CA",
                    StatusPagamento = "CA"
                });

            contexto.Agendamentoservicos.AddRange(
                new Agendamentoservico { IdAgendamento = 1, IdServico = 1, PrecoCobrado = 100m },
                new Agendamentoservico { IdAgendamento = 2, IdServico = 1, PrecoCobrado = 100m },
                new Agendamentoservico { IdAgendamento = 3, IdServico = 2, PrecoCobrado = 50m },
                new Agendamentoservico { IdAgendamento = 4, IdServico = 2, PrecoCobrado = 500m });

            contexto.SaveChanges();

            dashboardService = new DashboardService(contexto);
        }

        [TestCleanup]
        public void Finalizar()
        {
            contexto.Dispose();
        }

        [TestMethod]
        public void GetResumoServicos_DeveRetornarMaisUsadoMenosUsadoECategoriaMaisUsadaIgnorandoCancelados()
        {
            // Act
            var resumo = dashboardService.GetResumoServicos(1);

            // Assert
            Assert.IsNotNull(resumo);
            Assert.AreEqual("Corte", resumo.ServicoMaisUsadoNome);
            Assert.AreEqual(2, resumo.ServicoMaisUsadoQuantidade);
            Assert.AreEqual("Depilação", resumo.ServicoMenosUsadoNome);
            Assert.AreEqual(1, resumo.ServicoMenosUsadoQuantidade);
            Assert.AreEqual("Cabelo", resumo.CategoriaMaisUsadaNome);
            Assert.AreEqual(2, resumo.CategoriaMaisUsadaQuantidade);
        }

        [TestMethod]
        public void GetDistribuicaoPorServico_QuandoDadosValidos_DeveRetornarPercentuaisCalculados()
        {
            // Act
            var resultado = dashboardService.GetDistribuicaoPorServico(
                1, 1, "Genero", new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)).ToList();

            // Assert
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("Mulher", resultado[0].Label);
            Assert.AreEqual(2, resultado[0].Quantidade);
            Assert.AreEqual(100m, resultado[0].Percentual);
        }

        [TestMethod]
        public void GetDistribuicaoPorCategoria_QuandoServicoDeOutroEstabelecimento_DeveLancarArgumentException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
                dashboardService.GetDistribuicaoPorCategoria(
                    1, 3, "Genero", new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)));
        }

        [TestMethod]
        public void GetDistribuicaoPorServico_QuandoDataInicioMaiorQueDataFim_DeveLancarArgumentException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
                dashboardService.GetDistribuicaoPorServico(
                    1, 1, "Genero", new DateTime(2026, 5, 31), new DateTime(2026, 5, 1)));
        }

        [TestMethod]
        public void GetResumoFaturamento_DeveCalcularFaturamentoMensalEServicosIgnorandoCancelados()
        {
            // Act
            var resumo = dashboardService.GetResumoFaturamento(1, new DateTime(2026, 5, 25));

            // Assert
            Assert.IsNotNull(resumo);
            Assert.AreEqual(250m, resumo.FaturamentoMesAtual);
            Assert.AreEqual("Corte", resumo.ServicoMaisFaturaNome);
            Assert.AreEqual(200m, resumo.ServicoMaisFaturaValor);
            Assert.AreEqual("Depilação", resumo.ServicoMenosFaturaNome);
            Assert.AreEqual(50m, resumo.ServicoMenosFaturaValor);
            Assert.AreEqual(12, resumo.FaturamentoUltimos12Meses.Count());
        }

        [TestMethod]
        public void GetFaturamentoPorProfissional_DeveSomarApenasAgendamentosValidosNoPeriodo()
        {
            // Act
            var resultado = dashboardService.GetFaturamentoPorProfissional(
                1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)).ToList();

            // Assert
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("Fulano de Tal", resultado[0].Label);
            Assert.AreEqual(3, resultado[0].Quantidade);
            Assert.AreEqual(250m, resultado[0].Valor);
        }

        [TestMethod]
        public void GetFaturamentoPorServico_DeveRetornarSomaPorServicoOrdenadaPorValor()
        {
            // Act
            var resultado = dashboardService.GetFaturamentoPorServico(
                1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)).ToList();

            // Assert
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("Corte", resultado[0].Label);
            Assert.AreEqual(200m, resultado[0].Valor);
            Assert.AreEqual("Depilação", resultado[1].Label);
            Assert.AreEqual(50m, resultado[1].Valor);
        }

        [TestMethod]
        public void GetFaturamentoPorCategoria_DeveRetornarSomaPorCategoria()
        {
            // Act
            var resultado = dashboardService.GetFaturamentoPorCategoria(
                1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)).ToList();

            // Assert
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("Cabelo", resultado[0].Label);
            Assert.AreEqual(200m, resultado[0].Valor);
            Assert.AreEqual("Estética", resultado[1].Label);
            Assert.AreEqual(50m, resultado[1].Valor);
        }
    }
}