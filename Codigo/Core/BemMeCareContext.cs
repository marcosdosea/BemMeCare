using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Core;

public partial class BemMeCareContext : DbContext
{
    public BemMeCareContext()
    {
    }

    public BemMeCareContext(DbContextOptions<BemMeCareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agendamento> Agendamentos { get; set; }

    public virtual DbSet<Agendamentoservico> Agendamentoservicos { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Estabelecimento> Estabelecimentos { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Pessoa> Pessoas { get; set; }

    public virtual DbSet<Profissional> Profissionals { get; set; }

    public virtual DbSet<Servico> Servicos { get; set; }

    public virtual DbSet<Servicoprofissional> Servicoprofissionals { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=123456;database=BemMeCare");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("agendamento");

            entity.HasIndex(e => e.DataHoraAtendimento, "IDX_AgendamentoDataHoraAtendimento");

            entity.HasIndex(e => e.PrecoFinal, "IDX_AgendamentoPrecoFinal");

            entity.HasIndex(e => e.IdPessoaCliente, "fk_Agendamento_Pessoa1_idx");

            entity.HasIndex(e => e.IdPessoaMarcou, "fk_Agendamento_Pessoa2_idx");

            entity.HasIndex(e => new { e.IdProfissional, e.IdEstabelecimento }, "fk_Agendamento_ProfissionalEstabelecimento1_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DataHoraAtendimento)
                .HasColumnType("datetime")
                .HasColumnName("dataHoraAtendimento");
            entity.Property(e => e.DataHoraCriacao)
                .HasColumnType("datetime")
                .HasColumnName("dataHoraCriacao");
            entity.Property(e => e.DataHoraFimAtendimento)
                .HasColumnType("datetime")
                .HasColumnName("dataHoraFimAtendimento");
            entity.Property(e => e.DuracaoMinutos).HasColumnName("duracaoMinutos");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.IdPessoaCliente).HasColumnName("idPessoaCliente");
            entity.Property(e => e.IdPessoaMarcou).HasColumnName("idPessoaMarcou");
            entity.Property(e => e.IdProfissional).HasColumnName("idProfissional");
            entity.Property(e => e.Observacoes)
                .HasMaxLength(100)
                .HasColumnName("observacoes");
            entity.Property(e => e.PrecoFinal)
                .HasPrecision(10)
                .HasColumnName("precoFinal");
            entity.Property(e => e.StatusAgendamento)
                .HasComment("PE - PENDENTE\nCA - CANCELADO\nCO - COMPLETO\nSP - SINAL PAGO")
                .HasColumnType("enum('PE','CA','CO','SP')")
                .HasColumnName("statusAgendamento");
            entity.Property(e => e.StatusPagamento)
                .HasComment("MA - MARCADO\nCA - CANCELADO\nFI - FINALIZADO")
                .HasColumnType("enum('MA','CA','FI')")
                .HasColumnName("statusPagamento");
            entity.Property(e => e.ValorPagoSinal)
                .HasPrecision(10)
                .HasColumnName("valorPagoSinal");

            entity.HasOne(d => d.IdPessoaClienteNavigation).WithMany(p => p.AgendamentoIdPessoaClienteNavigations)
                .HasForeignKey(d => d.IdPessoaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Agendamento_Pessoa1");

            entity.HasOne(d => d.IdPessoaMarcouNavigation).WithMany(p => p.AgendamentoIdPessoaMarcouNavigations)
                .HasForeignKey(d => d.IdPessoaMarcou)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Agendamento_Pessoa2");

            entity.HasOne(d => d.Profissional).WithMany(p => p.Agendamentos)
                .HasForeignKey(d => new { d.IdProfissional, d.IdEstabelecimento })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Agendamento_ProfissionalEstabelecimento1");
        });

        modelBuilder.Entity<Agendamentoservico>(entity =>
        {
            entity.HasKey(e => new { e.IdServico, e.IdAgendamento }).HasName("PRIMARY");

            entity.ToTable("agendamentoservico");

            entity.HasIndex(e => e.IdAgendamento, "fk_ServicoAgendamento_Agendamento1_idx");

            entity.HasIndex(e => e.IdServico, "fk_ServicoAgendamento_Servico1_idx");

            entity.Property(e => e.IdServico).HasColumnName("idServico");
            entity.Property(e => e.IdAgendamento).HasColumnName("idAgendamento");
            entity.Property(e => e.PrecoCobrado)
                .HasPrecision(10)
                .HasColumnName("precoCobrado");

            entity.HasOne(d => d.IdAgendamentoNavigation).WithMany(p => p.Agendamentoservicos)
                .HasForeignKey(d => d.IdAgendamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ServicoAgendamento_Agendamento1");

            entity.HasOne(d => d.IdServicoNavigation).WithMany(p => p.Agendamentoservicos)
                .HasForeignKey(d => d.IdServico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ServicoAgendamento_Servico1");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.HasIndex(e => e.IdEstabelecimento, "fk_Categoria_Estabelecimento1_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");

            entity.HasOne(d => d.IdEstabelecimentoNavigation).WithMany(p => p.Categoria)
                .HasForeignKey(d => d.IdEstabelecimento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Categoria_Estabelecimento1");
        });

        modelBuilder.Entity<Estabelecimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("estabelecimento");

            entity.HasIndex(e => e.Cnpj, "cnpj_UNIQUE").IsUnique();

            entity.HasIndex(e => e.IdPessoa, "fk_Estabelecimento_Pessoa1_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bairro)
                .HasMaxLength(45)
                .HasColumnName("bairro");
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("cep");
            entity.Property(e => e.ChavePix)
                .HasMaxLength(45)
                .HasColumnName("chavePix");
            entity.Property(e => e.Cidade)
                .HasMaxLength(45)
                .HasColumnName("cidade");
            entity.Property(e => e.Cnpj)
                .HasMaxLength(14)
                .HasColumnName("cnpj");
            entity.Property(e => e.Complemento)
                .HasMaxLength(45)
                .HasColumnName("complemento");
            entity.Property(e => e.Contato)
                .HasMaxLength(45)
                .HasColumnName("contato");
            entity.Property(e => e.Estado)
                .HasMaxLength(45)
                .HasColumnName("estado");
            entity.Property(e => e.Foto)
                .HasColumnType("mediumblob")
                .HasColumnName("foto");
            entity.Property(e => e.IdPessoa).HasColumnName("idPessoa");
            entity.Property(e => e.Modalidade)
                .HasMaxLength(50)
                .HasColumnName("modalidade");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
            entity.Property(e => e.Numero)
                .HasMaxLength(45)
                .HasColumnName("numero");
            entity.Property(e => e.Rua)
                .HasMaxLength(45)
                .HasColumnName("rua");

            entity.HasOne(d => d.IdPessoaNavigation).WithMany(p => p.Estabelecimentos)
                .HasForeignKey(d => d.IdPessoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Estabelecimento_Pessoa1");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("horario");

            entity.HasIndex(e => new { e.IdProfissional, e.IdEstabelecimento }, "fk_Horario_ProfissionalEstabelecimento1_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiaSemana)
                .HasMaxLength(45)
                .HasColumnName("diaSemana");
            entity.Property(e => e.FimTurno)
                .HasColumnType("time")
                .HasColumnName("fimTurno");
            entity.Property(e => e.FimVigencia)
                .HasColumnType("date")
                .HasColumnName("fimVigencia");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.IdProfissional).HasColumnName("idProfissional");
            entity.Property(e => e.InicioTurno)
                .HasColumnType("time")
                .HasColumnName("inicioTurno");
            entity.Property(e => e.InicioVigencia)
                .HasColumnType("date")
                .HasColumnName("inicioVigencia");

            entity.HasOne(d => d.Profissional).WithMany(p => p.Horarios)
                .HasForeignKey(d => new { d.IdProfissional, d.IdEstabelecimento })
                .HasConstraintName("fk_Horario_ProfissionalEstabelecimento1");
        });

        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("pessoa");

            entity.HasIndex(e => e.Cor, "IDX_PessoaCor");

            entity.HasIndex(e => e.Genero, "IDX_PessoaGenero");

            entity.HasIndex(e => e.Sexualidade, "IDX_PessoaSexualidade");

            entity.HasIndex(e => e.Cpf, "cpf_UNIQUE").IsUnique();

            entity.HasIndex(e => e.Email, "email_UNIQUE").IsUnique();

            entity.HasIndex(e => e.Telefone, "telefone_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bairro)
                .HasMaxLength(45)
                .HasColumnName("bairro");
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("cep");
            entity.Property(e => e.Cidade)
                .HasMaxLength(45)
                .HasColumnName("cidade");
            entity.Property(e => e.Complemento)
                .HasMaxLength(45)
                .HasColumnName("complemento");
            entity.Property(e => e.Cor)
                .HasMaxLength(45)
                .HasColumnName("cor");
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .HasColumnName("cpf");
            entity.Property(e => e.DataDeNascimento)
                .HasColumnType("date")
                .HasColumnName("dataDeNascimento");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(45)
                .HasColumnName("estado");
            entity.Property(e => e.Foto)
                .HasColumnType("mediumblob")
                .HasColumnName("foto");
            entity.Property(e => e.Genero)
                .HasMaxLength(45)
                .HasColumnName("genero");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
            entity.Property(e => e.Numero)
                .HasMaxLength(45)
                .HasColumnName("numero");
            entity.Property(e => e.Rua)
                .HasMaxLength(45)
                .HasColumnName("rua");
            entity.Property(e => e.Senha)
                .HasMaxLength(45)
                .HasColumnName("senha");
            entity.Property(e => e.Sexualidade)
                .HasMaxLength(45)
                .HasColumnName("sexualidade");
            entity.Property(e => e.Telefone)
                .HasMaxLength(11)
                .HasColumnName("telefone");
        });

        modelBuilder.Entity<Profissional>(entity =>
        {
            entity.HasKey(e => new { e.IdPessoa, e.IdEstabelecimento }).HasName("PRIMARY");

            entity.ToTable("profissional");

            entity.HasIndex(e => e.IdEstabelecimento, "fk_PessoaEstabelecimento_Estabelecimento1_idx");

            entity.HasIndex(e => e.IdPessoa, "fk_PessoaEstabelecimento_Pessoa1_idx");

            entity.Property(e => e.IdPessoa).HasColumnName("idPessoa");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.ChavePix)
                .HasMaxLength(45)
                .HasColumnName("chavePix");
            entity.Property(e => e.DuracaoIntervaloTempo).HasColumnName("duracaoIntervaloTempo");
            entity.Property(e => e.EAtendente).HasColumnName("eAtendente");
            entity.Property(e => e.EProfissional).HasColumnName("eProfissional");
            entity.Property(e => e.EstaAtivo).HasColumnName("estaAtivo");
            entity.Property(e => e.GerirHorario).HasColumnName("gerirHorario");
            entity.Property(e => e.GerirServico).HasColumnName("gerirServico");
            entity.Property(e => e.ReceberPagamentoDireto).HasColumnName("receberPagamentoDireto");

            entity.HasOne(d => d.IdEstabelecimentoNavigation).WithMany(p => p.Profissionals)
                .HasForeignKey(d => d.IdEstabelecimento)
                .HasConstraintName("fk_PessoaEstabelecimento_Estabelecimento1");

            entity.HasOne(d => d.IdPessoaNavigation).WithMany(p => p.Profissionals)
                .HasForeignKey(d => d.IdPessoa)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_PessoaEstabelecimento_Pessoa1");

            entity.HasMany(d => d.IdCategoria).WithMany(p => p.Profissionals)
                .UsingEntity<Dictionary<string, object>>(
                    "Profissionalestabelecimentocategorium",
                    r => r.HasOne<Categorium>().WithMany()
                        .HasForeignKey("IdCategoria")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_ProfissionalEstabelecimentoCategoria_Categoria1"),
                    l => l.HasOne<Profissional>().WithMany()
                        .HasForeignKey("IdProfissional", "IdEstabelecimento")
                        .HasConstraintName("fk_ProfissionalEstabelecimentoCategoria_ProfissionalEstabelec1"),
                    j =>
                    {
                        j.HasKey("IdProfissional", "IdEstabelecimento", "IdCategoria").HasName("PRIMARY");
                        j.ToTable("profissionalestabelecimentocategoria");
                        j.HasIndex(new[] { "IdCategoria" }, "fk_ProfissionalEstabelecimentoCategoria_Categoria1_idx");
                        j.HasIndex(new[] { "IdProfissional", "IdEstabelecimento" }, "fk_ProfissionalEstabelecimentoCategoria_ProfissionalEstabel_idx");
                        j.IndexerProperty<uint>("IdProfissional").HasColumnName("idProfissional");
                        j.IndexerProperty<uint>("IdEstabelecimento").HasColumnName("idEstabelecimento");
                        j.IndexerProperty<uint>("IdCategoria").HasColumnName("idCategoria");
                    });
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("servico");

            entity.HasIndex(e => e.IdCategoria, "fk_Servico_Categoria1_idx");

            entity.HasIndex(e => e.IdEstabelecimento, "fk_Servico_Estabelecimento1_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Servicos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_Servico_Categoria1");

            entity.HasOne(d => d.IdEstabelecimentoNavigation).WithMany(p => p.Servicos)
                .HasForeignKey(d => d.IdEstabelecimento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Servico_Estabelecimento1");
        });

        modelBuilder.Entity<Servicoprofissional>(entity =>
        {
            entity.HasKey(e => new { e.IdProfissional, e.IdEstabelecimento, e.IdServico }).HasName("PRIMARY");

            entity.ToTable("servicoprofissional");

            entity.HasIndex(e => new { e.IdProfissional, e.IdEstabelecimento }, "fk_ProfissionalEstabelecimentoServico_ProfissionalEstabelec_idx");

            entity.HasIndex(e => e.IdServico, "fk_ProfissionalEstabelecimentoServico_Servico1_idx");

            entity.Property(e => e.IdProfissional).HasColumnName("idProfissional");
            entity.Property(e => e.IdEstabelecimento).HasColumnName("idEstabelecimento");
            entity.Property(e => e.IdServico).HasColumnName("idServico");
            entity.Property(e => e.Duracao).HasColumnName("duracao");
            entity.Property(e => e.ExigeAnamnese).HasColumnName("exigeAnamnese");
            entity.Property(e => e.FormAnamnese)
                .HasMaxLength(95)
                .HasColumnName("formAnamnese");
            entity.Property(e => e.PercentualSinal).HasColumnName("percentualSinal");
            entity.Property(e => e.Preco)
                .HasPrecision(10)
                .HasColumnName("preco");
            entity.Property(e => e.QuandoPagarSinal)
                .HasColumnType("enum('PRIMEIRO_AGENDAMENTO','TODO_AGENDAMENTO')")
                .HasColumnName("quandoPagarSinal");

            entity.HasOne(d => d.IdServicoNavigation).WithMany(p => p.Servicoprofissionals)
                .HasForeignKey(d => d.IdServico)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_ProfissionalEstabelecimentoServico_Servico1");

            entity.HasOne(d => d.Profissional).WithMany(p => p.Servicoprofissionals)
                .HasForeignKey(d => new { d.IdProfissional, d.IdEstabelecimento })
                .HasConstraintName("fk_ProfissionalEstabelecimentoServico_ProfissionalEstabelecim1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
