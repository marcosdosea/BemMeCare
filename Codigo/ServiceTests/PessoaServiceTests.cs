using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace ServiceTests;

[TestClass]
public class PessoaServiceTests
{
    private BemMeCareContext contexto = null!;
    private IPessoaService servicoPessoa = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var nomeBanco = $"BemMeCare_{Guid.NewGuid()}";

        var opcoes = new DbContextOptionsBuilder<BemMeCareContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        contexto = new BemMeCareContext(opcoes);

        contexto.Pessoas.AddRange(
            new Pessoa
            {
                Id = 1,
                Nome = "Machado de Assis",
                Email = "machado.de.assis@example.com",
                Senha = "senha123",
                Cpf = "123.456.789-00",
                Telefone = "(11) 98765-4321",
                DataDeNascimento = new DateTime(1917, 12, 31),
                Cor = "Branca",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Cep = "12345-678",
                Rua = "Rua Exemplo",
                Bairro = "Bairro Exemplo",
                Numero = "123",
                Complemento = "Apto 101",
                Cidade = "Cidade Exemplo",
                Estado = "Estado Exemplo"
            },
            new Pessoa
            {
                Id = 2,
                Nome = "Ian S. Sommervile",
                Email = "ian.s.sommervile@example.com",
                Senha = "senha456",
                Cpf = "987.654.321-00",
                Telefone = "(21) 98765-4321",
                DataDeNascimento = new DateTime(1935, 12, 31),
                Cor = "Branca",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Cep = "54321-876",
                Rua = "Rua Exemplo 2",
                Bairro = "Bairro Exemplo 2",
                Numero = "456",
                Complemento = "Apto 202",
                Cidade = "Cidade Exemplo 2",
                Estado = "Estado Exemplo 2"
            },
            new Pessoa
            {
                Id = 3,
                Nome = "Gleford Myers",
                Email = "gleford.myers@example.com",
                Senha = "senha789",
                Cpf = "555.555.555-55",
                Telefone = "(31) 98765-4321",
                DataDeNascimento = new DateTime(1900, 11, 20),
                Cor = "Branca",
                Genero = "Masculino",
                Sexualidade = "Heterossexual",
                Cep = "98765-432",
                Rua = "Rua Exemplo 3",
                Bairro = "Bairro Exemplo 3",
                Numero = "789",
                Complemento = "Apto 303",
                Cidade = "Cidade Exemplo 3",
                Estado = "Estado Exemplo 3"
            });

        contexto.SaveChanges();

        servicoPessoa = new PessoaService(contexto);
    }

    [TestCleanup]
    public void Finalizar()
    {
        contexto.Dispose();
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveRetornarIdDoPessoaCriado()
    {
        // Arrange
        var pessoa = new Pessoa
        {
            Id = 4,
            Nome = "Graciliano Ramos",
            Email = "graciliano.ramos@example.com",
            Senha = "senha123",
            Cpf = "111.111.111-11",
            Telefone = "(41) 98765-4321",
            DataDeNascimento = new DateTime(1900, 12, 25),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Cep = "11111-111",
            Rua = "Rua Exemplo 4",
            Bairro = "Bairro Exemplo 4",
            Numero = "1000",
            Complemento = "Apto 404",
            Cidade = "Cidade Exemplo 4",
            Estado = "Estado Exemplo 4"
        };

        // Act
        var idRetornado = servicoPessoa.Create(pessoa);

        // Assert
        Assert.AreEqual(4u, idRetornado);
    }

    [TestMethod]
    public void Create_QuandoDadosSaoValidos_DeveArmazenarPessoa()
    {
        // Arrange
        var pessoaEsperada = new Pessoa
        {
            Id = 4,
            Nome = "Graciliano Ramos",
            Email = "graciliano.ramos@example.com",
            Senha = "senha123",
            Cpf = "111.111.111-11",
            Telefone = "(41) 98765-4321",
            DataDeNascimento = new DateTime(1900, 12, 25),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Cep = "11111-111",
            Rua = "Rua Exemplo 4",
            Bairro = "Bairro Exemplo 4",
            Numero = "1000",
            Complemento = "Apto 404",
            Cidade = "Cidade Exemplo 4",
            Estado = "Estado Exemplo 4"
        };

        // Act
        servicoPessoa.Create(pessoaEsperada);

        // Assert
        var pessoaCriada = servicoPessoa.Get(pessoaEsperada.Id);

        Assert.IsNotNull(pessoaCriada);
        Assert.AreEqual(pessoaEsperada.Id, pessoaCriada.Id);
        Assert.AreEqual(pessoaEsperada.Nome, pessoaCriada.Nome);
        Assert.AreEqual(pessoaEsperada.Email, pessoaCriada.Email);
        Assert.AreEqual(pessoaEsperada.Senha, pessoaCriada.Senha);
        Assert.AreEqual(pessoaEsperada.Cpf, pessoaCriada.Cpf);
        Assert.AreEqual(pessoaEsperada.Telefone, pessoaCriada.Telefone);
        Assert.AreEqual(pessoaEsperada.DataDeNascimento, pessoaCriada.DataDeNascimento);
        Assert.AreEqual(pessoaEsperada.Cor, pessoaCriada.Cor);
        Assert.AreEqual(pessoaEsperada.Genero, pessoaCriada.Genero);
        Assert.AreEqual(pessoaEsperada.Sexualidade, pessoaCriada.Sexualidade);
        Assert.AreEqual(pessoaEsperada.Cep, pessoaCriada.Cep);
        Assert.AreEqual(pessoaEsperada.Rua, pessoaCriada.Rua);
        Assert.AreEqual(pessoaEsperada.Bairro, pessoaCriada.Bairro);
        Assert.AreEqual(pessoaEsperada.Numero, pessoaCriada.Numero);
        Assert.AreEqual(pessoaEsperada.Complemento, pessoaCriada.Complemento);
        Assert.AreEqual(pessoaEsperada.Cidade, pessoaCriada.Cidade);
        Assert.AreEqual(pessoaEsperada.Estado, pessoaCriada.Estado);
    }

    [TestMethod]
    public void Delete_QuandoPessoaExiste_DeveRemoverPessoa()
    {
        // Act
        servicoPessoa.Delete(2);

        // Assert
        var pessoaExcluida = servicoPessoa.Get(2);
        var pessoasRestantes = servicoPessoa.GetAll().ToList();

        Assert.IsNull(pessoaExcluida);
        Assert.HasCount(2, pessoasRestantes);
        Assert.IsFalse(pessoasRestantes.Any(pessoa => pessoa.Id == 2));
    }

    [TestMethod]
    public void Delete_QuandoPessoaNaoExiste_NaoDeveAlterarOsDados()
    {
        // Arrange
        var pessoasIniciais = servicoPessoa.GetAll().ToList();

        // Act
        servicoPessoa.Delete(999);

        // Assert
        var pessoasFinais = servicoPessoa.GetAll().ToList();

        Assert.HasCount(pessoasIniciais.Count, pessoasFinais);
        CollectionAssert.AreEquivalent(
            pessoasIniciais.Select(pessoa => pessoa.Id).ToList(),
            pessoasFinais.Select(pessoa => pessoa.Id).ToList());
    }

    [TestMethod]
    public void Edit_QuandoPessoaExiste_DeveAtualizarSeusDados()
    {
        // Arrange
        var pessoa = servicoPessoa.Get(1);

        Assert.IsNotNull(pessoa);

        pessoa!.Nome = "Machado de Assis Atualizado";
        pessoa.Email = "machado.atualizado@example.com";
        pessoa.Senha = "novasenha123";
        pessoa.Cpf = "123.456.789-00";
        pessoa.Telefone = "(11) 98765-4321";
        pessoa.DataDeNascimento = new DateTime(1950, 11, 21);
        pessoa.Cor = "Branca";
        pessoa.Genero = "Masculino";
        pessoa.Sexualidade = "Heterossexual";
        pessoa.Cep = "22222-222";
        pessoa.Rua = "Rua Exemplo 1";
        pessoa.Bairro = "Bairro Exemplo 1";
        pessoa.Numero = "100";
        pessoa.Complemento = "Apto 101";
        pessoa.Cidade = "Cidade Exemplo 1";
        pessoa.Estado = "Estado Exemplo 1";

        // Act
        servicoPessoa.Edit(pessoa);

        // Assert
        var pessoaAtualizada = servicoPessoa.Get(1);

        Assert.IsNotNull(pessoaAtualizada);
        Assert.AreEqual(1u, pessoaAtualizada.Id);
        Assert.AreEqual("Machado de Assis Atualizado", pessoaAtualizada.Nome);
        Assert.AreEqual("machado.atualizado@example.com", pessoaAtualizada.Email);
        Assert.AreEqual("novasenha123", pessoaAtualizada.Senha);
        Assert.AreEqual("123.456.789-00", pessoaAtualizada.Cpf);
        Assert.AreEqual("(11) 98765-4321", pessoaAtualizada.Telefone);
        Assert.AreEqual(new DateTime(1950, 11, 21), pessoaAtualizada.DataDeNascimento);
        Assert.AreEqual("Branca", pessoaAtualizada.Cor);
        Assert.AreEqual("Masculino", pessoaAtualizada.Genero);
        Assert.AreEqual("Heterossexual", pessoaAtualizada.Sexualidade);
        Assert.AreEqual("22222-222", pessoaAtualizada.Cep);
        Assert.AreEqual("Rua Exemplo 1", pessoaAtualizada.Rua);
        Assert.AreEqual("Bairro Exemplo 1", pessoaAtualizada.Bairro);
        Assert.AreEqual("100", pessoaAtualizada.Numero);
        Assert.AreEqual("Apto 101", pessoaAtualizada.Complemento);
        Assert.AreEqual("Cidade Exemplo 1", pessoaAtualizada.Cidade);
        Assert.AreEqual("Estado Exemplo 1", pessoaAtualizada.Estado);
    }

    [TestMethod]
    public void Get_QuandoIdExiste_DeveRetornarPessoaCorrespondente()
    {
        // Act
        var pessoa = servicoPessoa.Get(1);

        // Assert
        Assert.IsNotNull(pessoa);
        Assert.AreEqual(1u, pessoa.Id);
        Assert.AreEqual("Machado de Assis", pessoa.Nome);
        Assert.AreEqual("machado.de.assis@example.com", pessoa.Email);
        Assert.AreEqual("senha123", pessoa.Senha);
        Assert.AreEqual("123.456.789-00", pessoa.Cpf);
        Assert.AreEqual("(11) 98765-4321", pessoa.Telefone);
        Assert.AreEqual(new DateTime(1917, 12, 31), pessoa.DataDeNascimento);
        Assert.AreEqual("Branca", pessoa.Cor);
        Assert.AreEqual("Masculino", pessoa.Genero);
        Assert.AreEqual("Heterossexual", pessoa.Sexualidade);
        Assert.AreEqual("12345-678", pessoa.Cep);
        Assert.AreEqual("Rua Exemplo", pessoa.Rua);
        Assert.AreEqual("Bairro Exemplo", pessoa.Bairro);
        Assert.AreEqual("123", pessoa.Numero);
        Assert.AreEqual("Apto 101", pessoa.Complemento);
        Assert.AreEqual("Cidade Exemplo", pessoa.Cidade);
        Assert.AreEqual("Estado Exemplo", pessoa.Estado);
    }

    [TestMethod]
    public void Get_QuandoIdNaoExiste_DeveRetornarNulo()
    {
        // Act
        var pessoa = servicoPessoa.Get(999);

        // Assert
        Assert.IsNull(pessoa);
    }

    [TestMethod]
    public void GetAll_QuandoExistemPessoas_DeveRetornarTodasAsPessoas()
    {
        // Act
        var pessoas = servicoPessoa.GetAll().ToList();

        // Assert
        Assert.IsNotNull(pessoas);
        Assert.HasCount(3, pessoas);

        CollectionAssert.AreEquivalent(
            new uint[] { 1, 2, 3 },
            pessoas.Select(pessoa => pessoa.Id).ToList());
    }

    [TestMethod]
    public void GetAll_QuandoNaoHaPessoas_DeveRetornarColecaoVazia()
    {
        // Arrange
        contexto.Pessoas.RemoveRange(contexto.Pessoas);
        contexto.SaveChanges();

        // Act
        var pessoas = servicoPessoa.GetAll().ToList();

        // Assert
        Assert.IsNotNull(pessoas);
        Assert.IsEmpty(pessoas);
    }

    [TestMethod]
    public void GetByNome_QuandoExisteCorrespondencia_DeveRetornarPessoasCorrespondentes()
    {
        // Act
        var pessoas = servicoPessoa.GetByNome("Machado").ToList();

        // Assert
        Assert.HasCount(1, pessoas);
        Assert.AreEqual("Machado de Assis", pessoas[0].Nome);
    }

    [TestMethod]
    public void GetByNome_QuandoNomeCorrespondeAoInicio_DeveRetornarTodosAsPessoasCorrespondentes()
    {
        // Arrange
        contexto.Pessoas.Add(new Pessoa
        {
            Id = 4,
            Nome = "Machado de Almeida",
            Email = "machado.de.almeida@example.com",
            Senha = "senha321",
            Cpf = "222.222.222-22",
            Telefone = "(51) 98765-4321",
            DataDeNascimento = new DateTime(1920, 5, 15),
            Cor = "Branca",
            Genero = "Masculino",
            Sexualidade = "Heterossexual",
            Cep = "22222-222",
            Rua = "Rua Exemplo 5",
            Bairro = "Bairro Exemplo 5",
            Numero = "500",
            Complemento = "Apto 505",
            Cidade = "Cidade Exemplo 5",
            Estado = "Estado Exemplo 5"
        });

        contexto.SaveChanges();

        // Act
        var pessoas = servicoPessoa.GetByNome("Machado").ToList();

        // Assert
        Assert.HasCount(2, pessoas);
        Assert.IsTrue(
            pessoas.All(pessoa => pessoa.Nome.StartsWith("Machado")));
    }

    [TestMethod]
    public void GetByNome_QuandoNaoExisteCorrespondencia_DeveRetornarColecaoVazia()
    {
        // Act
        var pessoas = servicoPessoa.GetByNome("José").ToList();

        // Assert
        Assert.IsNotNull(pessoas);
        Assert.IsEmpty(pessoas);
    }

    [TestMethod]
    public void GetByNome_QuandoNomeEstaVazio_DeveRetornarTodasAsPessoas()
    {
        // Act
        var pessoas = servicoPessoa.GetByNome(string.Empty).ToList();

        // Assert
        Assert.HasCount(3, pessoas);
    }
}
