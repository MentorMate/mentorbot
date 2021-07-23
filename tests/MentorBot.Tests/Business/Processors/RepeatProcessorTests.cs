using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="RepeatProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class RepeatProcessorTests
    {
        private RepeatProcessor _processor;

        [TestInitialize]
        public void TestInitialize()
        {
            _processor = new RepeatProcessor();
        }

        [DataRow("Repeat I am a test", "I am a test", DisplayName = "Test short repeat")]
        [DataRow("Repeat after me I am a long test", "I am a long test", DisplayName = "Test long repeat")]
        [DataRow("Repeat after this I am a long test", "after this I am a long test", DisplayName = "Test odd repeat")]
        [DataRow("@mentorbot Repeat test", "test", DisplayName = "Test with @mentorbot")]
        [DataRow("", "Repeat command can not recognize some segments.", DisplayName = "Unknown text")]
        [DataTestMethod]
        public async Task WhenAskedItShouldRepeat(string phrase, string expectedResult)
        {
            var info = new TextDeconstructionInformation(phrase, null);
            var result = await _processor.ProcessCommandAsync(info, new ChatEvent(), null, null);
            Assert.AreEqual(expectedResult, result.Text);
        }

        [TestMethod]
        public void RepeatProcessorSubjectShouldBeRepeat()
        {
            Assert.AreEqual(_processor.Subject, "Repeat");
        }

        [TestMethod]
        public void RepeatProcessorCanParseTimeString()
        {
            Assert.AreEqual(RepeatProcessor.GetTime("10min"), 600000);
            Assert.AreEqual(RepeatProcessor.GetTime("6sec"), 6000);
            Assert.AreEqual(RepeatProcessor.GetTime("2h"), 7200000);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task WhenAskedItShouldRepeatAsync()
        {
            var sender = new ChatEventMessageSender();
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            var chat = new ChatEvent { Space = space, Message = message };
            var responder = Substitute.For<IAsyncResponder>();
            var info = new TextDeconstructionInformation(
                "Repeat delay 100 Test",
                "Time",
                SentenceTypes.Unknown,
                new Dictionary<string, string[]> { { "Time", new[] { "100" } } },
                null,
                1.0);

            var result = await _processor.ProcessCommandAsync(info, chat, responder, null);

            responder
                .DidNotReceive()
                .SendMessageAsync("Test", Arg.Any<GoogleChatAddress>());

            Thread.Sleep(150);

            responder
                .Received()
                .SendMessageAsync("Test", Arg.Is<GoogleChatAddress>(it => it.Space == space && it.Sender == sender));
        }

#pragma warning restore CS4014
    }
}
