using Microsoft.AspNetCore.Identity;

namespace LoginPage.Utilitarios
{
    public static class AuxiliarDeSenha
    {
        public static string GerarHashDaSenha(string senha)
        {
            var gerador = new PasswordHasher<object>();
            return gerador.HashPassword(null, senha);
        }

        // compara a senha digitada com o hash salvo no banco
        public static bool VerificarSenha(string senha, string SenhaHash)
        {
            var verificador = new PasswordHasher<object>();
            var resultado = verificador.VerifyHashedPassword(null, senha, SenhaHash);
            return resultado == PasswordVerificationResult.Success;
        }
    }
}
