using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace Service.Tests;

[TestClass]
public class EstabelecimentoServiceTests
{
    private BemMeCareContext contexto = null!;
    private IEstabelecimentoService servicoEstabelecimento = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var nomeBanco = $"Banco_{Guid.NewGuid()}";

        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        contexto = new BemMeCareContext(opcoes);

        contexto.Estabelecimentos.AddRange(
            new Estabelecimento
            {
                Id = 1,
                Nome = "Salão Beleza Pura",
                Modalidade = "Modalidade-1",
                Cnpj = "12345678000190",
                Cep = "49000001",
                Rua = "Rua Exemplo",
                Numero = "123",
                Complemento = "Sala 101",
                Bairro = "Bairro Exemplo",
                Cidade = "Cidade Exemplo",
                Estado = "Estado Exemplo",
                Contato = "(79) 98765-4321",
                ChavePix = "12345678000190",
                IdPessoa = 1
            },
            new Estabelecimento
            {
                Id = 2,
                Nome = "Barbearia Estilo Clássico",
                Modalidade = "Modalidade-2",
                Cnpj = "98765432000110",
                Cep = "49000002",
                Rua = "Rua Exemplo 2",
                Numero = "456",
                Complemento = "Loja 2",
                Bairro = "Bairro Exemplo 2",
                Cidade = "Cidade Exemplo 2",
                Estado = "Estado Exemplo 2",
                Contato = "(79) 99876-5432",
                ChavePix = "barbearia@example.com",
                IdPessoa = 2
            },
            new Estabelecimento
            {
                Id = 3,
                Nome = "Clínica Bem Estar",
                Modalidade = "Modalidade-3",
                Cnpj = "11222333000181",
                Cep = "49000003",
                Rua = "Rua Exemplo 3",
                Numero = "789",
                Complemento = "Andar 3",
                Bairro = "Bairro Exemplo 3",
                Cidade = "Cidade Exemplo 3",
                Estado = "Estado Exemplo 3",
                Contato = "(79) 98888-7777",
                ChavePix = "(79) 98888-7777",
                IdPessoa = 3
            });

        contexto.SaveChanges();

        servicoEstabelecimento = new EstabelecimentoService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoEstabelecimentoCriado()
    {
        // Arrange
        var estabelecimento = new Estabelecimento
        {
            Id = 4,
            Nome = "Nova Barbearia",
            Modalidade = "Modalidade-4",
            Cnpj = "12345678000190",
            Cep = "49000001",
            Rua = "Rua Exemplo",
            Numero = "123",
            Complemento = "Sala 101",
            Bairro = "Bairro Exemplo",
            Cidade = "Cidade Exemplo",
            Estado = "Estado Exemplo",
            Contato = "(79) 98765-4321",
            ChavePix = "12345678000190",
            IdPessoa = 1
        };

        // Act
        var idRetornado = servicoEstabelecimento.Create(estabelecimento);

        // Assert
        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarEstabelecimento()
    {
        // Arrange
        var estabelecimentoEsperado = new Estabelecimento
        {
            Id = 4,
            Nome = "Nova Barbearia",
            Modalidade = "Modalidade-4",
            Cnpj = "12345678000190",
            Cep = "49000001",
            Rua = "Rua Exemplo",
            Numero = "123",
            Complemento = "Sala 101",
            Bairro = "Bairro Exemplo",
            Cidade = "Cidade Exemplo",
            Estado = "Estado Exemplo",
            Contato = "(79) 98765-4321",
            ChavePix = "12345678000190",
            IdPessoa = 1
        };

        // Act
        servicoEstabelecimento.Create(estabelecimentoEsperado);

        // Assert
        var estabelecimentoCriado = servicoEstabelecimento.Get(4);

        Assert.IsNotNull(estabelecimentoCriado);
        Assert.AreEqual(estabelecimentoEsperado.Id, estabelecimentoCriado.Id);
        Assert.AreEqual(estabelecimentoEsperado.Nome, estabelecimentoCriado.Nome);
        Assert.AreEqual(estabelecimentoEsperado.Modalidade, estabelecimentoCriado.Modalidade);
        Assert.AreEqual(estabelecimentoEsperado.Cnpj, estabelecimentoCriado.Cnpj);
        Assert.AreEqual(estabelecimentoEsperado.Cep, estabelecimentoCriado.Cep);
        Assert.AreEqual(estabelecimentoEsperado.Rua, estabelecimentoCriado.Rua);
        Assert.AreEqual(estabelecimentoEsperado.Numero, estabelecimentoCriado.Numero);
        Assert.AreEqual(estabelecimentoEsperado.Complemento, estabelecimentoCriado.Complemento);
        Assert.AreEqual(estabelecimentoEsperado.Bairro, estabelecimentoCriado.Bairro);
        Assert.AreEqual(estabelecimentoEsperado.Cidade, estabelecimentoCriado.Cidade);
        Assert.AreEqual(estabelecimentoEsperado.Estado, estabelecimentoCriado.Estado);
        Assert.AreEqual(estabelecimentoEsperado.Contato, estabelecimentoCriado.Contato);
        Assert.AreEqual(estabelecimentoEsperado.ChavePix, estabelecimentoCriado.ChavePix);
        Assert.AreEqual(estabelecimentoEsperado.IdPessoa, estabelecimentoCriado.IdPessoa);
    }

    [TestMethod]
    public void Delete_QuandoEstabelecimentoExiste_DeveRemoverEstabelecimento()
    {
        // Act
        servicoEstabelecimento.Delete(2);

        // Assert
        var estabelecimentoExcluido = servicoEstabelecimento.Get(2);
        var estabelecimentosRestantes = servicoEstabelecimento.GetAll().ToList();

        Assert.IsNull(estabelecimentoExcluido);
        Assert.HasCount(2, estabelecimentosRestantes);
        Assert.IsFalse(estabelecimentosRestantes.Any(estabelecimento => estabelecimento.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoEstabelecimentoNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var estabelecimentosIniciais = servicoEstabelecimento.GetAll().ToList();

        // Act
        servicoEstabelecimento.Delete(999);

        // Assert
        var estabelecimentosFinais = servicoEstabelecimento.GetAll().ToList();

        Assert.HasCount(estabelecimentosIniciais.Count, estabelecimentosFinais);
        CollectionAssert.AreEquivalent(
            estabelecimentosIniciais.Select(estabelecimento => estabelecimento.Id).ToList(),
            estabelecimentosFinais.Select(estabelecimento => estabelecimento.Id).ToList());
    }

    [TestMethod]
    public void Edit_QuandoEstabelecimentoExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var estabelecimento = servicoEstabelecimento.Get(3);

        Assert.IsNotNull(estabelecimento);

        estabelecimento!.Nome = "Clínica Vida Plena";
        estabelecimento.Modalidade = "Modalidade-Atualizada";
        estabelecimento.Cnpj = "55666777000199";
        estabelecimento.Cep = "49000099";
        estabelecimento.Rua = "Rua Atualizada";
        estabelecimento.Numero = "999";
        estabelecimento.Complemento = "Sala 10";
        estabelecimento.Bairro = "Bairro Atualizado";
        estabelecimento.Cidade = "Cidade Atualizada";
        estabelecimento.Estado = "Estado Atualizado";
        estabelecimento.Contato = "(79) 97777-6666";
        estabelecimento.ChavePix = "vidaplena@example.com";
        estabelecimento.IdPessoa = 2;

        // Act
        servicoEstabelecimento.Edit(estabelecimento);

        // Assert
        var estabelecimentoAtualizado = servicoEstabelecimento.Get(3);

        Assert.IsNotNull(estabelecimentoAtualizado);
        Assert.AreEqual("Clínica Vida Plena", estabelecimentoAtualizado.Nome);
        Assert.AreEqual("Modalidade-Atualizada", estabelecimentoAtualizado.Modalidade);
        Assert.AreEqual("55666777000199", estabelecimentoAtualizado.Cnpj);
        Assert.AreEqual("49000099", estabelecimentoAtualizado.Cep);
        Assert.AreEqual("Rua Atualizada", estabelecimentoAtualizado.Rua);
        Assert.AreEqual("999", estabelecimentoAtualizado.Numero);
        Assert.AreEqual("Sala 10", estabelecimentoAtualizado.Complemento);
        Assert.AreEqual("Bairro Atualizado", estabelecimentoAtualizado.Bairro);
        Assert.AreEqual("Cidade Atualizada", estabelecimentoAtualizado.Cidade);
        Assert.AreEqual("Estado Atualizado", estabelecimentoAtualizado.Estado);
        Assert.AreEqual("(79) 97777-6666", estabelecimentoAtualizado.Contato);
        Assert.AreEqual("vidaplena@example.com", estabelecimentoAtualizado.ChavePix);
        Assert.AreEqual(2u, estabelecimentoAtualizado.IdPessoa);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarEstabelecimentoCorrespondente()
    {
        // Act
        var estabelecimento = servicoEstabelecimento.Get(1);

        // Assert
        Assert.IsNotNull(estabelecimento);
        Assert.AreEqual(1u, estabelecimento.Id);
        Assert.AreEqual("Salão Beleza Pura", estabelecimento.Nome);
        Assert.AreEqual("Modalidade-1", estabelecimento.Modalidade);
        Assert.AreEqual("12345678000190", estabelecimento.Cnpj);
        Assert.AreEqual("49000001", estabelecimento.Cep);
        Assert.AreEqual("Rua Exemplo", estabelecimento.Rua);
        Assert.AreEqual("123", estabelecimento.Numero);
        Assert.AreEqual("Sala 101", estabelecimento.Complemento);
        Assert.AreEqual("Bairro Exemplo", estabelecimento.Bairro);
        Assert.AreEqual("Cidade Exemplo", estabelecimento.Cidade);
        Assert.AreEqual("Estado Exemplo", estabelecimento.Estado);
        Assert.AreEqual("(79) 98765-4321", estabelecimento.Contato);
        Assert.AreEqual("12345678000190", estabelecimento.ChavePix);
        Assert.AreEqual(1u, estabelecimento.IdPessoa);
    }

    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        // Act
        var estabelecimento = servicoEstabelecimento.Get(999);

        // Assert
        Assert.IsNull(estabelecimento);
    }

    [TestMethod]
    public void GetAll_QuandoExistemEstabelecimentos_DeveRetornarTodosOsEstabelecimentos()
    {
        // Act
        var estabelecimentos = servicoEstabelecimento.GetAll().ToList();

        // Assert
        Assert.IsNotNull(estabelecimentos);
        Assert.HasCount(3, estabelecimentos);

        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2, 3 },
            estabelecimentos.Select(estabelecimento => estabelecimento.Id).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaEstabelecimentos_DeveRetornarColecaoVazia()
    {
        // Arrange
        contexto.Estabelecimentos.RemoveRange(contexto.Estabelecimentos);
        contexto.SaveChanges();

        // Act
        var estabelecimentos = servicoEstabelecimento.GetAll().ToList();

        // Assert
        Assert.IsNotNull(estabelecimentos);
        Assert.IsEmpty(estabelecimentos);
    }

    [TestMethod]
    public void GetByNome_QuandoExisteCorrespondencia_DeveRetornarEstabelecimentosCorrespondentes()
    {
        // Act
        var estabelecimentos = servicoEstabelecimento.GetByNome("Salão").ToList();

        // Assert
        Assert.HasCount(1, estabelecimentos);
        Assert.AreEqual("Salão Beleza Pura", estabelecimentos[0].Nome);
    }

    [TestMethod]
    public void GetByNome_QuandoNomeCorrespondeAoInicio_DeveRetornarTodosOsEstabelecimentosCorrespondentes()
    {
        // Arrange
        contexto.Estabelecimentos.Add(new Estabelecimento
        {
            Id = 4,
            Nome = "Salão Beleza Premium",
            Modalidade = "Modalidade-4",
            Cnpj = "44555666000177",
            Cep = "49000004",
            Rua = "Rua Exemplo 4",
            Numero = "321",
            Complemento = "Sala 4",
            Bairro = "Bairro Exemplo 4",
            Cidade = "Cidade Exemplo 4",
            Estado = "Estado Exemplo 4",
            Contato = "(79) 96666-5555",
            ChavePix = "premium@example.com",
            IdPessoa = 1
        });

        contexto.SaveChanges();

        // Act
        var estabelecimentos = servicoEstabelecimento.GetByNome("Salão").ToList();

        // Assert
        Assert.HasCount(2, estabelecimentos);
        Assert.IsTrue(
            estabelecimentos.All(estabelecimento => estabelecimento.Nome.StartsWith("Salão")));
    }

    [TestMethod]
    public void GetByNome_QuandoNaoExisteCorrespondencia_DeveRetornarColecaoVazia()
    {
        // Act
        var estabelecimentos = servicoEstabelecimento.GetByNome("Academia").ToList();

        // Assert
        Assert.IsNotNull(estabelecimentos);
        Assert.IsEmpty(estabelecimentos);
    }

    [TestMethod]
    public void GetByNome_QuandoNomeEstaVazio_DeveRetornarTodosOsEstabelecimentos()
    {
        // Act
        var estabelecimentos = servicoEstabelecimento.GetByNome(string.Empty).ToList();

        // Assert
        Assert.HasCount(3, estabelecimentos);
    }
}