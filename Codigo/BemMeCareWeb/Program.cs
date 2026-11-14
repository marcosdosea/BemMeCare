using Core;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using Service;

namespace BemMeCareWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<BemMeCareContext>(
                options => options.UseMySQL(builder.Configuration.GetConnectionString("BemMeCareDatabase") ?? throw new InvalidOperationException("Conexão com o banco de dados não foi possível. Favor entrar em contato com o Administrador.")));

            builder.Services.AddTransient<ICategoriaService, CategoriaService>();
            builder.Services.AddTransient<IEstabelecimentoService, EstabelecimentoService>();
            builder.Services.AddTransient<IProfissionalService, ProfissionalService>();
            builder.Services.AddTransient<IPessoaService, PessoaService>();
            builder.Services.AddTransient<IServicoService, ServicoService>();
            builder.Services.AddTransient<IHorarioService, HorarioService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();


            app.Run();
        }
    }
}