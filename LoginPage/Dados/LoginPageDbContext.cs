using LoginPage.Modelo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace LoginPage.Dados
{
    public class LoginPageDbContext :DbContext //dbcontext é a simbolização da base em memória
    {
        //tabelas
        public DbSet<UsuarioModelo> Usuarios { get; set; }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString: "Server=localhost;Database=LoginPageDB;Trusted_Connection=True;TrustServerCertificate=True");
            base.OnConfiguring(optionsBuilder);
        }

    }
}
