using System;
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
            _processor = new CalendarProcessor(_connector, () => TimeZoneInfo.Local);
        }

        [TestMethod]
        public void CalendarProcessorSubjectShouldBeMeetings()
        {
            Assert.AreEqual(_processor.Subject, "Meetings");
        }

#pragma warning disable CS4014

        [DataRow("What is my next meeting", "jhon.doe@gmail.com", DisplayName = "next meeting")]
        [DataRow("get the next meeting", "robin.hood@gmail.com", DisplayName = "get next")]
        [TestMethod]
        public async Task WhenAskedForMeeting_ShouldUseConnector(string phrase, string mail)
        {
            var info = new TextDeconstructionInformation("What is my next meeting", null);
            var chatEvent = GetChatEvent(phrase, mail);

            _connector.GetNextMeetingAsync(mail).Returns((Event)null);

            var result = await _processor.ProcessCommandAsync(info, chatEvent, null, null);

            _connector.Received().GetNextMeetingAsync(mail);

            Assert.AreEqual(result.Text, "Can not find your next event! You may have no events or the service user account do not see your calendar.");
        }

        [TestMethod]
        public async Task WhenAskedForMeeting_ShouldReturnEvent()
        {
            var info = new TextDeconstructionInformation("What is my next meeting", null);
            var chatEvent = GetChatEvent("What is my next meeting", "jhon.doe@gmail.com");
            var item = new Event
            {
                Summary = "Board Meeting",
                Start = new EventDateTime
                {
                    DateTime = new DateTime(2000, 1, 1, 1, 1, 1)
                },
                ConferenceData = new ConferenceData
                {
                    EntryPoints = new []
                    {
                        new EntryPoint
                        {
                            Uri = "https://domain.com/link"
                        }
                    }
                }
            };

            _connector.GetNextMeetingAsync("jhon.doe@gmail.com").Returns(item);

            var result = await _processor.ProcessCommandAsync(info, chatEvent, null, null);
            var value = result.Cards[0].Sections[0].Widgets[0].KeyValue;
            Assert.AreEqual(value.Content, "Board Meeting");
            Assert.AreEqual(value.BottomLabel, "01:01");
            Assert.AreEqual(value.Icon, "INVITE");
            Assert.AreEqual(value.Button.TextButton.Text, "JOIN");
            Assert.AreEqual(value.Button.TextButton.OnClick.OpenLink.Url, "https://domain.com/link");
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
