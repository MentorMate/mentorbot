using System;
using System.Threading.Tasks;

using MentorBot.Functions.Connectors.BingMaps;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using TimeZoneConverter;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="LocalTimeProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class LocalTimeProcessorTests
    {
        private LocalTimeProcessor _processor;
        private IBingMapsClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = Substitute.For<IBingMapsClient>();
            _processor = new LocalTimeProcessor(
                _client,
                () => TZConvert.GetTimeZoneInfo("FLE Standard Time"),
                () => new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc));
        }

        [TestMethod]
        public void LocalTimeProcessorSubjectShouldBeTime()
        {
            Assert.AreEqual(_processor.Subject, "Time");
        }

        [DataRow("What is the current time in Mars", "Mars", null, "The current time in Mars was not found.", DisplayName = "Time in unknown location")]
        [DataRow("What is the current time", null, null, "The current time is 03:00.", DisplayName = "Time in FLE")]
        [DataRow("What is the local time in Moscow", "Moscow", "Europe/Moscow", "The current time in Moscow is 04:00.", DisplayName = "Time in Moscow")]
        [DataTestMethod]
        public async Task WhenAskedForCurrentTime(string phrase, string word, string data, string expectedResult)
        {
            var info = new TextDeconstructionInformation(phrase, null);
            var timeZoneData = data == null ? null : new BingMapsClient.TimeZoneData { GenericName = data };

            _client.QueryAsync(word).Returns(timeZoneData);

            var result = await _processor.ProcessCommandAsync(info, GetChatEvent(phrase), null, null);

            Assert.AreEqual(expectedResult, result.Text);
        }

        public static ChatEvent GetChatEvent(string text) =>
            new ChatEvent
            {
                Message = new ChatEventMessage
                {
                    Text = text
                }
            };
    }
}
