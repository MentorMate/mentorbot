namespace MentorBot.Functions.App.SmtpClient
{
    #pragma warning disable DE0005

    /// <summary>Hide base SmtpClient so we can do a unit test and a abstraction for portability..</summary>
    public sealed class SmtpClientBase : System.Net.Mail.SmtpClient, ISmtpClient
    {
    }

    #pragma warning restore DE0005
}
