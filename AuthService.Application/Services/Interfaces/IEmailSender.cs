namespace AuthService.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string recipient, string subject, string body);
        string GetPrettyConfirmation(string confirmationLink);
    }
}
