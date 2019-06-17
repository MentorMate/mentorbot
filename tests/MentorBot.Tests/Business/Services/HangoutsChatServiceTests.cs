using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public class HangoutsChatServiceTests
    {
        private HangoutsChatService _service;
        private ICognitiveService _cognitiveService;
        private IAsyncResponder _asyncResponder;

        [TestInitialize]
        public void TestInitialize()
        {
            _cognitiveService = Substitute.For<ICognitiveService>();
            _asyncResponder = Substitute.For<IAsyncResponder>();
            _service = new HangoutsChatService(_cognitiveService, _asyncResponder);
        }

        [TestMethod]
        public async Task WhenNoCommandReturnMessageAsync()
        {
            var dummyChatEvent = GetChatEvent();

            var result = await _service.BasicAsync(dummyChatEvent);

            Assert.AreEqual(Messages.UnknownCommandText, result.Output.Text);
        }

        [TestMethod]
        public async Task WhenCommandConfidenceRatingLowReturnMessageAsync()
        {
            var dummyChatEvent = GetChatEvent();
            var dymmtCognitiveTextAnalysisResult = GetCognitiveTextAnalysisResult();

            _cognitiveService.ProcessAsync(dummyChatEvent).Returns(dymmtCognitiveTextAnalysisResult);

            var result = await _service.BasicAsync(dummyChatEvent);

            Assert.AreEqual(Messages.UnknownCommandText, result.Output.Text);
        }

        private static ChatEvent GetChatEvent() =>
            new ChatEvent();

        private static CognitiveTextAnalysisResult GetCognitiveTextAnalysisResult() =>
            new CognitiveTextAnalysisResult(null, null, null);
    }
}
