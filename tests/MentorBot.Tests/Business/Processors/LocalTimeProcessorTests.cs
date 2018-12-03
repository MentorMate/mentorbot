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
            _processor = new LocalTimeProcessor();
        }

        [DataRow("What is the current time in Mars", "The current time in Mars was not found.", DisplayName = "Time in unknow location")]
        [DataRow("What is the current time", "The current time is 1:00:00 AM UTC.", DisplayName = "Time in UTC")]
        [DataRow("What is the local time in Moscow", "The current time in Moscow is 4:00:00 AM.", DisplayName = "Time in Moscow")]
        [DataTestMethod]
        public async Task WhenAskedForCurrentTime(string phrase, string expectedResult)
        {
            var today = new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            var info = new TextDeconstructionInformation(phrase, null, SentenceTypes.Command);
            var result = await _processor.ProcessCommandAsync(info, GetChatEvent(today, phrase), null);
            Assert.AreEqual(expectedResult, result.Text);
        }

        public static ChatEvent GetChatEvent(DateTime time, string text) =>
            new ChatEvent
            {
                EventTime = time,
                Message = new ChatEventMessage
                {
                    CreateTime = time,
                    Text = text
                }
            };
    }
}
