using System;
using System.Threading.Tasks;

using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="LocalTimeProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class LocalTimeProcessorTests
    {
        private LocalTimeProcessor _processor;

        [TestInitialize]
        public void TestInitialize()
        {
            _processor = new LocalTimeProcessor(
                () => TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"),
                () => new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc));
        }

        [DataRow("What is the current time in Mars", "The current time in Mars was not found.", DisplayName = "Time in unknow location")]
        [DataRow("What is the current time", "The current time is 03:00.", DisplayName = "Time in FLE")]
        [DataRow("What is the local time in Moscow", "The current time in Moscow is 04:00.", DisplayName = "Time in Moscow")]
        [DataTestMethod]
        public async Task WhenAskedForCurrentTime(string phrase, string expectedResult)
        {
            var info = new TextDeconstructionInformation(phrase, null);
            var result = await _processor.ProcessCommandAsync(info, GetChatEvent(phrase), null);
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
