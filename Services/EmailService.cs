using System.Net.Mail;
using System.Net;

namespace ApiFarmacia.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> EnviarEmailConfirmacionAsync(string email, string nombre, string tokenConfirmacion)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:3000";
            var confirmUrl = $"{frontendUrl}/confirmar-email?token={tokenConfirmacion}";

            var subject = "Confirma tu cuenta - Farmacia";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 ¡Bienvenido a Farmacia!</h1>
        </div>
        <div class='content'>
            <h2>Hola {nombre},</h2>
            <p>Gracias por registrarte en nuestra plataforma. Estamos emocionados de tenerte con nosotros.</p>
            <p>Para completar tu registro y activar tu cuenta, por favor confirma tu dirección de email haciendo clic en el siguiente botón:</p>
            <div style='text-align: center;'>
                <a href='{confirmUrl}' class='button'>Confirmar Email</a>
            </div>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='background: #e9ecef; padding: 10px; border-radius: 5px; word-break: break-all;'>{confirmUrl}</p>
            <p><strong>Este enlace expirará en 24 horas.</strong></p>
            <p>Si no creaste esta cuenta, puedes ignorar este email.</p>
        </div>
        <div class='footer'>
            <p>© 2025 Farmacia. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de confirmación a {Email}", email);
            return false;
        }
    }

    public async Task<bool> EnviarEmailRecuperacionPasswordAsync(string email, string nombre, string tokenRecuperacion)
    {
        try
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?token={tokenRecuperacion}";

            var subject = "Recuperación de Contraseña - Farmacia";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Recuperación de Contraseña</h1>
        </div>
        <div class='content'>
            <h2>Hola {nombre},</h2>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Si fuiste tú, haz clic en el siguiente botón para crear una nueva contraseña:</p>
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Restablecer Contraseña</a>
            </div>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='background: #e9ecef; padding: 10px; border-radius: 5px; word-break: break-all;'>{resetUrl}</p>
            <p><strong>Este enlace expirará en 1 hora.</strong></p>
            <p>Si no solicitaste restablecer tu contraseña, ignora este email. Tu contraseña permanecerá sin cambios.</p>
        </div>
        <div class='footer'>
            <p>© 2025 Farmacia. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de recuperación a {Email}", email);
            return false;
        }
    }

    public async Task<bool> EnviarEmailBienvenidaAsync(string email, string nombre)
    {
        try
        {
            var subject = "¡Bienvenido a Farmacia! 🎉";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ ¡Cuenta Confirmada!</h1>
        </div>
        <div class='content'>
            <h2>¡Hola {nombre}!</h2>
            <p>Tu cuenta ha sido confirmada exitosamente. Ya puedes disfrutar de todos nuestros servicios.</p>
            <p>Gracias por unirte a nuestra comunidad. Estamos aquí para ayudarte.</p>
            <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
        </div>
        <div class='footer'>
            <p>© 2025 Farmacia. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de bienvenida a {Email}", email);
            return false;
        }
    }

    private async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var smtpUsername = emailSettings["SmtpUsername"];
            var smtpPassword = emailSettings["SmtpPassword"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"] ?? "Farmacia";

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            await Task.Run(() => client.Send(mailMessage));
            _logger.LogInformation("Email enviado exitosamente a {Email}", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Email}", destinatario);
            return false;
        }
    }
}
