using System.Threading.Tasks;

using MentorBot.Business.Services;
using MentorBot.Core;
using MentorBot.Core.Abstract.Processor;
using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.TextAnalytics;

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

            Assert.AreEqual(Messages.UnknownCommandText, result.Text);
        }

        [TestMethod]
        public async Task WhenCommandConfidenceRatingLowReturnMessageAsync()
        {
            var dummyChatEvent = GetChatEvent();
            var dymmtCognitiveTextAnalysisResult = GetCognitiveTextAnalysisResult();

            _cognitiveService.ProcessAsync(dummyChatEvent).Returns(dymmtCognitiveTextAnalysisResult);

            var result = await _service.BasicAsync(dummyChatEvent);

            Assert.AreEqual(Messages.UnknownCommandText, result.Text);
        }

        private static ChatEvent GetChatEvent() =>
            new ChatEvent();

        private static CognitiveTextAnalysisResult GetCognitiveTextAnalysisResult() =>
            new CognitiveTextAnalysisResult(null, null, 0.99);
    }
}
