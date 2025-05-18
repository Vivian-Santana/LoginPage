using LoginPage.Dados;
using LoginPage.DTOs;
using LoginPage.Modelo;
using LoginPage.Utilitarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata.Ecma335;

namespace LoginPage.Rotas
{
    public static class RotaUsuarios
    {
        public static void RotasUsuarios(this WebApplication app)
        {
            //map group
            var route = app.MapGroup(prefix: "usuario");

            route.MapPost(pattern: "criar", async (UsuarioRequest dto, LoginPageDbContext context) =>
            {
                var senhaCriptografada = AuxiliarDeSenha.GerarHashDaSenha(dto.Senha);
                var usuario = new UsuarioModelo(dto.Name, senhaCriptografada); // cria uma nova instância de usuário a partir da requisição

                await context.AddAsync(usuario); // adiciona o usuário ao contexto (prepara para inserção no banco)
                await context.SaveChangesAsync(); // salva (commita) as alterações no banco de dados

                return Results.Created($"/usuarios/{usuario.Id}", new RespostaUsuario(usuario.Id, usuario.Name));
            });

            //pega lista de usuários
            route.MapGet(pattern:"pegar", async (LoginPageDbContext context) =>
            {
                //inferencia de tipo   (tipo é List<UsuarioModelo>)
                var usuarios = await context.Usuarios
                .Select(usuarios => new RespostaUsuario(usuarios.Id, usuarios.Name))
                .ToListAsync();

                return Results.Ok(usuarios);
            });

            //pega usuario pelo id
            route.MapGet("/{id:guid}/pegarPorId", async (Guid id, LoginPageDbContext context) =>
            {
                var usuario = await context.Usuarios.FindAsync(id);

                return usuario is not null
                    ? Results.Ok(new RespostaUsuario(usuario.Id, usuario.Name))
                    : Results.NotFound();
            });
            
            route.MapPut(pattern: "{id:guid}/alterar",
                async (Guid id, UsuarioRequest req, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

                    if (usuarioEncontrado == null)
                        return Results.NotFound();

                    usuarioEncontrado.MudarNome(req.Name);
                    await context.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi alterado com sucesso!");
                });

            // soft delete marca como inativo na coluna status
            route.MapDelete(pattern: "{id:guid}/desativar",
                async (Guid id, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);
                    
                    if (usuarioEncontrado == null)
                        return Results.NotFound();

                    //seta como inativo
                     usuarioEncontrado.SetInativo();
                     await context.SaveChangesAsync();

                     return Results.Ok($"Usuário com ID {id} foi desativado.");
                });

            // Hard delete - remoção permanente
            route.MapDelete(pattern:"{id:guid}/deletar",
                async (Guid id, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

                    if (usuarioEncontrado == null)
                        return Results.NotFound();

                    context.Usuarios.Remove(usuarioEncontrado);
                    await context.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi removido permanentemente.");
                });
        }
    }
}
