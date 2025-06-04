using LoginPage.Modelo;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginPage.Utilitarios
{
    public class TokenService
    {
        public static string GerarToken(UsuarioModelo usuario, IConfiguration config)
        {
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!); // Obtém a chave secreta do appsettings.json para gerar a assinatura do token

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Name),
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario)
                }),

                Expires = DateTime.UtcNow.AddHours(2), // Define o tempo de expiração do token (2h a partir da geração)

                // Define as credenciais de assinatura do token (HMAC SHA256) para garantir sua integridade e autenticidade
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),

                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]
            };

            // Cria o token JWT com base na descrição e retorna como string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
