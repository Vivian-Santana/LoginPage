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
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!); //Obtém a chave secreta do appsettings.json

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Name),
                }),

                Expires = DateTime.UtcNow.AddHours(2), // tempo de validade do token

                //proteção contra alteração do token sem a chave secreta
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),

                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]
            };

            //cria e retorna o token gerado
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
