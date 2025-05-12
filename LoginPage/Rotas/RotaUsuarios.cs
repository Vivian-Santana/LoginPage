using LoginPage.Dados;
using LoginPage.Modelo;
using Microsoft.EntityFrameworkCore;

namespace LoginPage.Rotas
{
    public static class RotaUsuarios
    {
        public static void RotasUsuarios(this WebApplication app)
        {
            //map group
            var route = app.MapGroup(prefix: "usuario");

            route.MapPost(pattern: "criar", async (UsuarioRequest req, LoginPageDbContext context) =>
            {
                var usuario = new UsuarioModelo(req.name); // cria uma nova instância de usuário a partir da requisição
                await context.AddAsync(usuario); // adiciona o usuário ao contexto (prepara para inserção no banco)
                await context.SaveChangesAsync(); // salva (commita) as alterações no banco de dados

            });

            //pega lista de usuários
            route.MapGet(pattern:"pegar", async (LoginPageDbContext context) =>
            {
                //inferencia de tipo   (tipo é List<UsuarioModelo>)
                var usuarios = await context.Usuarios.ToListAsync(); 
                return Results.Ok(usuarios);
            });

            /*
            app.MapGet("/dbcheck", async (LoginPageDbContext db) =>
            {
                var canConnect = await db.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok("Conexão com o banco de dados estabelecida com sucesso!")
                    : Results.Problem("Falha ao conectar ao banco de dados.");
            });
            */
        }
    }
}
