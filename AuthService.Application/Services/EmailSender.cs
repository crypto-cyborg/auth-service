using AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AuthService.Application.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        private string? _originEmail;
        private string? _password;
        private string? _smtpAdress;
        private int _port;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            Configure();
        }

        private void Configure()
        {
            _originEmail = _configuration.GetSection("smtp")["originEmail"];
            _password = _configuration.GetSection("smtp")["password"];
            _smtpAdress = _configuration.GetSection("smtp")["address"];
            var portString = _configuration.GetSection("smtp")["port"];

            var isPortParsed = int.TryParse(portString, out _port);

            if (
                _originEmail is null ||
                _password is null ||
                _smtpAdress is null ||
                _smtpAdress is null ||
                !isPortParsed)
            {
                throw new Exception("Smtp configuration missing");
            }
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var client = new SmtpClient(_smtpAdress, _port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_originEmail, _password)
            };

            var message = new MailMessage()
            {
                From = new(_originEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.CC.Add(new(recipient));

            await client.SendMailAsync(message);
        }

        public string GetPrettyConfirmation(string confirmationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirm Your Email</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            padding: 20px;
            background-color: #ffffff;
            border: 1px solid #dddddd;
            border-radius: 5px;
        }}
        .header {{
            text-align: center;
            padding-bottom: 20px;
        }}
        .header h1 {{
            margin: 0;
            color: #333333;
        }}
        .content {{
            padding: 20px 0;
            text-align: center;
        }}
        .content p {{
            margin: 0 0 15px;
            color: #555555;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            font-size: 16px;
            color: #ffffff;
            background-color: #007bff;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
        .footer {{
            margin-top: 20px;
            text-align: center;
            color: #aaaaaa;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Confirm Your Email</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>Thank you for registering. Please click the button below to confirm your email address:</p>
            <a href=""{confirmationLink}"" class=""button"">Confirm Email</a>
            <p>If you did not request this, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>© 2024 Your Company. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}