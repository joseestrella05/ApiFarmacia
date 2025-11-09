using ApiFarmacia.Models;
using System.Security.Claims;

namespace ApiFarmacia.Services;

public interface ITokenService
{
    string GenerarToken(Usuario usuario);
    string GenerarRefreshToken();
    ClaimsPrincipal? ValidarToken(string token);
}