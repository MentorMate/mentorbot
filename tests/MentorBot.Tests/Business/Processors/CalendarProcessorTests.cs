using System.Threading.Tasks;

using Google.Apis.Calendar.v3.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="CalendarProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class CalendarProcessorTests
    {
        private CalendarProcessor _processor;
        private IGoogleCalendarConnector _connector;

        [TestInitialize]
        public void TestInitialize()
        {
            _connector = Substitute.For<IGoogleCalendarConnector>();
            _processor = new CalendarProcessor(_connector);
        }

        #pragma warning disable CS4014

        [DataRow("What is my next meeting", "jhon.doe@gmail.com", DisplayName = "next meeting")]
        [DataRow("get the next meeting", "robin.hood@gmail.com", DisplayName = "get next")]
        [TestMethod]
        public async Task WhenAskedForMeeting_ShouldUseConnector(string phrase, string mail)
        {
            var info = new TextDeconstructionInformation("What is my next meeting", null, SentenceTypes.Question);
            var chatEvent = GetChatEvent(phrase, mail);

            _connector.GetNextMeetingAsync(mail).Returns((Event)null);

            var result = await _processor.ProcessCommandAsync(info, chatEvent, null);

            _connector.Received().GetNextMeetingAsync(mail);
        }

        #pragma warning restore CS4014

        public static ChatEvent GetChatEvent(string text, string mail) =>
            new ChatEvent
            {
                Message = new ChatEventMessage
                {
                    Sender = new ChatEventMessageSender
                    {
                        Email = mail
                    },
                    Text = text
                }
            };
    }
}
