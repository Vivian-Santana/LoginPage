using AutoMapper;
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

            route.MapPost(pattern: "criar", async (UsuarioRequest request, LoginPageDbContext db, IMapper mapper) =>
            {
                var senhaCriptografada = AuxiliarDeSenha.GerarHashDaSenha(request.Senha);
                var usuario = new UsuarioModelo(request.Name, senhaCriptografada); // cria uma nova instância de usuário a partir da requisição

                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();

                var resposta = mapper.Map<RespostaUsuario>(usuario);
                return Results.Created($"/usuarios/{usuario.Id}", resposta);

            });

            //pega lista de usuários
            route.MapGet(pattern:"pegar", async (LoginPageDbContext db, IMapper mapper) =>
            {
                //inferencia de tipo   (tipo é List<UsuarioModelo>)
                var usuarios = await db.Usuarios.ToListAsync();
                var resposta = mapper.Map<List<RespostaUsuario>>(usuarios);

                return Results.Ok(resposta);
            });

            //pega usuario pelo id
            route.MapGet("/{id:guid}/pegarPorId", async (Guid id, LoginPageDbContext db, IMapper mapper) =>
            {
                var usuario = await db.Usuarios.FindAsync(id);
                if(usuario is null) return Results.NotFound();
                
                var resposta = mapper.Map<RespostaUsuario>(usuario);
                return Results.Ok(resposta);

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

            app.MapPost("/login", async (UsuarioRequest request, LoginPageDbContext db) =>
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Name == request.Name);

                if (usuario is null || !AuxiliarDeSenha.VerificarSenha(request.Senha, usuario.SenhaHash))
                    return Results.Problem(
                        detail: "As credenciais fornecidas são inválidas.",
                        statusCode: StatusCodes.Status401Unauthorized
                    );

                return Results.Ok(new { mensagem = "Login realizado com sucesso!" });
            });
        }
    }
}
