namespace ApiFarmacia.Services;

public interface IEmailService
{
    Task<bool> EnviarEmailConfirmacionAsync(string email, string nombre, string tokenConfirmacion);
    Task<bool> EnviarEmailRecuperacionPasswordAsync(string email, string nombre, string tokenRecuperacion);
    Task<bool> EnviarEmailBienvenidaAsync(string email, string nombre);
}
