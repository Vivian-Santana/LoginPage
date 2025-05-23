using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LoginPage.Utilitarios
{
    public class ValidadorDeSenha
    {
        public static bool SenhaValida(string senha, out List<string> erros)
        {
            erros = new List<string>();

            if (string.IsNullOrWhiteSpace(senha))
            {
                erros.Add("A senha não pode ser vazia.");
                return false;
            }

            if (senha.Length < 8)
                erros.Add("A senha deve ter no mínimo 8 caracteres.");

            if (!senha.Any(char.IsUpper))
                erros.Add("A senha deve conter pelo menos uma letra maiúscula.");

            if (!senha.Any(char.IsLower))
                erros.Add("A senha deve conter pelo menos uma letra minúscula.");

            if (!senha.Any(char.IsDigit))
                erros.Add("A senha deve conter pelo menos um número.");

            if (!senha.Any(c => "!@#$%^&*()_+-=[]{}|;:',.<>?".Contains(c)))
                erros.Add("A senha deve conter pelo menos um caractere especial.");

            return erros.Count == 0;
        }
    }
}
