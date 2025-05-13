using LoginPage.Dados;
using LoginPage.Modelo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

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

            route.MapPut(pattern: "{id:guid}",
                async (Guid id, UsuarioRequest req, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

                    if (usuarioEncontrado == null)
                        return Results.NotFound();

                    usuarioEncontrado.MudarNome(req.name);
                    await context.SaveChangesAsync();

                    return Results.Ok(usuarioEncontrado);
                });

            //soft delete
            route.MapDelete(pattern: "{id:guid}",
                async (Guid id, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);
                    
                    if (usuarioEncontrado == null)
                        return Results.NotFound();

                    //seta como inativo
                     usuarioEncontrado.SetInativo();
                     await context.SaveChangesAsync();

                     return Results.Ok(usuarioEncontrado);
                });
        }
    }
}
