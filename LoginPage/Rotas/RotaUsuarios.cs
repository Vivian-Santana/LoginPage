using AutoMapper;
using Azure.Core;
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

            route.MapPost(pattern: "criar", async (UsuarioRequest req, LoginPageDbContext db, IMapper mapper) =>
            {
                //validação da senha dentro das regras de criação de senha
                if (!ValidadorDeSenha.SenhaValida(req.Senha, out var erros))
                    return Results.BadRequest(new { Erros = erros }); // se não estiver dentro das regras de validação, retorna os erros

                var senhaCriptografada = AuxiliarDeSenha.GerarHashDaSenha(req.Senha);
                var usuario = new UsuarioModelo(req.Name, senhaCriptografada); // cria uma nova instância de usuário a partir da requisição

                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();

                var resposta = mapper.Map<RespostaUsuario>(usuario);
                return Results.Created($"/usuarios/{usuario.Id}", new {Mensage = "Usuário criado com sucesso!"});
            });

            //Login autenticação
            route.MapPost(pattern: "/login", async (UsuarioRequest request, LoginPageDbContext db) =>
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Name == request.Name);

                if (usuario is null || !AuxiliarDeSenha.VerificarSenha(request.Senha, usuario.SenhaHash))
                    return Results.Problem(
                        detail: "Usuário ou senha inválidos!",
                        statusCode: StatusCodes.Status401Unauthorized
                    );

                if (usuario.Status == false)
                    return Results.Problem(
                    detail: "Usuário não encontrado.",
                    statusCode: StatusCodes.Status404NotFound
                );

                return Results.Ok(new { mensagem = "Login realizado com sucesso!" });
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
                if(usuario is null)
                    return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );

                var resposta = mapper.Map<RespostaUsuario>(usuario);
                return Results.Ok(resposta);

            });

            //atualizar senha
            route.MapPut("{id:guid}/alterarSenha",
            async (Guid id, AlterarSenhaRequest req, LoginPageDbContext context) =>
            {
                var usuario = await context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null || usuario.Status == false)
                {
                    return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                // Verifica senha atual
                var senhaCorreta = AuxiliarDeSenha.VerificarSenha(
                    req.SenhaAtual, usuario.SenhaHash
                );

                if (!senhaCorreta)
                {
                    return Results.BadRequest(new{Mensagem = "Senha incorreta!"});
                }

                // Valida nova senha
                if (!ValidadorDeSenha.SenhaValida(req.NovaSenha, out var erros))
                {
                    return Results.BadRequest(new
                    {
                        Mensagem = "Nova senha não atende aos requisitos de segurança!",
                        Erros = erros
                    });
                }

                // Atualizar dados
                usuario.MudarSenha(AuxiliarDeSenha.GerarHashDaSenha(req.NovaSenha));

                await context.SaveChangesAsync();

                return Results.Ok(new{Mensagem = "Senha alterada com sucesso!"});
            });

            //reativa usuario
            route.MapPut(pattern: "{id:guid}/reativar",
                async (Guid id, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios
                        .FirstOrDefaultAsync(usuario => usuario.Id == id);

                    if (usuarioEncontrado == null)
                    {
                        return Results.Problem(
                            detail: "Usuário não encontrado.",
                            statusCode: StatusCodes.Status404NotFound
                        );
                    }

                    if (usuarioEncontrado.Status == true)
                    {
                        return Results.BadRequest(new{Mensagem = "O usuário já está ativo."});
                    }

                    usuarioEncontrado.SetAtivo();
                    await context.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi reativado.");
                });

            // soft delete marca como inativo na coluna status
            route.MapDelete(pattern: "{id:guid}/desativar",
                async (Guid id, LoginPageDbContext context) =>
                {
                    var usuarioEncontrado = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);
                    
                    if (usuarioEncontrado == null)
                        return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );

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
                        return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );

                    context.Usuarios.Remove(usuarioEncontrado);
                    await context.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi removido permanentemente.");
                });

        }
    }
}
