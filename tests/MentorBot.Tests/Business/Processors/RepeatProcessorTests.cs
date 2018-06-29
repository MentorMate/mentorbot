using System.Threading.Tasks;
using MentorBot.Business.Processors;
using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.TextAnalytics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Processors
{
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
        [DataTestMethod]
        public async Task WhenAskedItShouldRepeat(string phrase, string expectedResult)
        {
            var info = new TextDeconstructionInformation(phrase, null, SentenceTypes.Command);
            var result = await _processor.ProcessCommandAsync(info, new ChatEvent(), null);
            Assert.AreEqual(expectedResult, result.Text);
        }
    }
}
