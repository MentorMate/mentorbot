using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Default e-mail service.</summary>
    public interface IMailService
    {
        /// <summary>Gets a value indicating whether this mail service is active.</summary>
        bool IsActive { get; }

        /// <summary>Sends an email a list of recipents.</summary>
        Task SendMailAsync(string subject, string message, params string[] emails);
    }
}
