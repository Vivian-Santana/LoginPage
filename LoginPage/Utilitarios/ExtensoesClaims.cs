using System.Security.Claims;

namespace LoginPage.Utilitarios
{
    public static class ExtensoesClaims
    {
        //método de extensão para extrair o ID do token
        public static Guid ObterUsuarioId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id is not null ? Guid.Parse(id) : Guid.Empty;
        }
    }
}
