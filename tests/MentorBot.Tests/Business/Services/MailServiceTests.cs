// cSpell:ignore Jhon
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions.App.SmtpClient;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class MailServiceTests
    {
        private ISmtpClient _client;
        private SmtpOptions _options;
        private MailService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _options = new SmtpOptions("smtp.domain.com", 1, "jhon.doe@domain.com", "Jhon Doe", "D", "E", false);
            _client = Substitute.For<ISmtpClient>();
            _service = new MailService(_client, _options);
        }

        [TestMethod]
        public async Task MailServiceSend()
        {
            await _service.SendMailAsync("Test", "Msg", "test@domain.com");
            _client
                .Received()
                .SendMailAsync(Arg.Is<MailMessage>(it =>
                    it.From.Address == "jhon.doe@domain.com" &&
                    it.To[0].Address == "test@domain.com" &&
                    it.Subject == "Test" &&
                    it.Body == "Msg" &&
                    it.IsBodyHtml &&
                    it.BodyEncoding == Encoding.UTF8))
                .Wait();
        }

        [TestMethod]
        public void MailServiceDisposses()
        {
            _service.Dispose();
            _client.Received().Dispose();
        }
    }
}
