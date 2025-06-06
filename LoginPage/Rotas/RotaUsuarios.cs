﻿using AutoMapper;
using LoginPage.Dados;
using LoginPage.DTOs;
using LoginPage.Modelo;
using LoginPage.Utilitarios;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            route.MapPost(pattern: "/login", async (UsuarioRequest request, LoginPageDbContext db, IConfiguration config) =>
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Name == request.Name && u.Status);

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

                //Gera o token JWT
                var token = TokenService.GerarToken(usuario, config);

                return Results.Ok(new
                {
                    mensagem = "Login realizado com sucesso!",
                    token,
                    usuario = new { usuario.Id, usuario.Name }
                });

            });

            //pega lista de usuários (Apenas Admin)
            route.MapGet(pattern:"pegar",
                [Authorize(Roles = "Admin")]
                async (LoginPageDbContext db, IMapper mapper) =>
            {
                //inferencia de tipo   (tipo é List<UsuarioModelo>)
                var usuarios = await db.Usuarios.ToListAsync();
                var resposta = mapper.Map<List<RespostaUsuario>>(usuarios);

                return Results.Ok(resposta);
            });

            route.MapGet("/{id:guid}/pegarPorId",
                [Authorize]
                async (Guid id, LoginPageDbContext db, IMapper mapper, ClaimsPrincipal user) =>
            {
                // Dados do token
                var usuarioIdDoToken = user.ObterUsuarioId();
                var nomeDoUsuarioToken = user.Identity?.Name;
                var isAdmin = user.IsInRole("Admin");
                Console.WriteLine($"Usuário autenticado: {nomeDoUsuarioToken}, ID: {usuarioIdDoToken}");

                if (!isAdmin && usuarioIdDoToken != id)
                {
                    return Results.Problem(
                        detail: "Usuário não autorizado.",
                        statusCode: StatusCodes.Status401Unauthorized // Acesso negado se o ID do token não bater com o ID da URL
                    );
                }

                var usuario = await db.Usuarios.FindAsync(id);
                
                if(usuario is null)
                { 
                    return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                var resposta = mapper.Map<RespostaUsuario>(usuario);
                return Results.Ok(resposta);

            })
            .RequireAuthorization(); //exige um token JWT válido

            // apenas o usuario pode alterar a propria senha
            route.MapPut("{id:guid}/alterarSenha",
            async (Guid id, AlterarSenhaRequest req, LoginPageDbContext db, ClaimsPrincipal user) =>
            {
                var usuarioIdDoToken = user.ObterUsuarioId();

                if (usuarioIdDoToken != id)
                {
                    return Results.Problem(
                        detail: "Usuário não autorizado.",
                        statusCode: StatusCodes.Status401Unauthorized
                    );
                }

                var usuario = await db.Usuarios.
                    FirstOrDefaultAsync(u => u.Id == id);

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

                await db.SaveChangesAsync();

                return Results.Ok(new{Mensagem = "Senha alterada com sucesso!"});
            })
            .RequireAuthorization();

            //reativa usuario
            route.MapPut(pattern: "{id:guid}/reativar",
                [Authorize]
                async (Guid id, LoginPageDbContext db, ClaimsPrincipal user) =>
                {
                    var usuarioIdDoToken = user.ObterUsuarioId();
                    var isAdmin = user.IsInRole("Admin");

                    if (!isAdmin && usuarioIdDoToken != id)
                    {
                        return Results.Problem(
                            detail: "Usuário não autorizado.",
                            statusCode: StatusCodes.Status401Unauthorized
                        );
                    }

                    var usuarioEncontrado = await db.Usuarios
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
                    await db.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi reativado.");
                })
                .RequireAuthorization();

            // soft delete marca como inativo na coluna status
            route.MapDelete(pattern: "{id:guid}/desativar",
                [Authorize]
                async (Guid id, LoginPageDbContext db, ClaimsPrincipal user) =>
                {
                    var usuarioIdDoToken = user.ObterUsuarioId();
                    var isAdmin = user.IsInRole("Admin");

                    if (!isAdmin && usuarioIdDoToken != id)
                    {
                        return Results.Problem(
                            detail: "Usuário não autorizado.",
                            statusCode: StatusCodes.Status401Unauthorized
                        );
                    }
                    var usuarioEncontrado = await db.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);
                    
                    if (usuarioEncontrado == null)
                        return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );

                    //seta como inativo
                    usuarioEncontrado.SetInativo();
                     await db.SaveChangesAsync();

                     return Results.Ok($"Usuário com ID {id} foi desativado.");
                })
                .RequireAuthorization();

            // Hard delete - remoção permanente
            route.MapDelete(pattern:"{id:guid}/deletar",
                [Authorize]
                async (Guid id, LoginPageDbContext db, ClaimsPrincipal user) =>
                {
                    var usuarioIdDoToken = user.ObterUsuarioId();
                    var isAdmin = user.IsInRole("Admin");

                    if (!isAdmin && usuarioIdDoToken != id)
                    {
                        return Results.Problem(
                            detail: "Usuário não autorizado.",
                            statusCode: StatusCodes.Status401Unauthorized
                        );
                    }
                    var usuarioEncontrado = await db.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

                    if (usuarioEncontrado == null)
                        return Results.Problem(
                        detail: "Usuário não encontrado.",
                        statusCode: StatusCodes.Status404NotFound
                    );

                    db.Usuarios.Remove(usuarioEncontrado);
                    await db.SaveChangesAsync();

                    return Results.Ok($"Usuário com ID {id} foi removido permanentemente.");
                })
                .RequireAuthorization();

        }
    }
}
