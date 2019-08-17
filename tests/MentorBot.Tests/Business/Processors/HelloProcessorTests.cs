using System.Threading.Tasks;

using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class HelloProcessorTests
    {
        private HelloProcessor _processor;

        [TestInitialize]
        public void TestInitialize()
        {
            _processor = new HelloProcessor();
        }

        [TestMethod]
        public void HelloProcessorSubjectShoudBeHelp()
        {
            Assert.AreEqual(_processor.Subject, "Hello");
        }

        [TestMethod]
        public void HelloProcessorNameShouldBeOk()
        {
            Assert.AreEqual(_processor.Name, "Hello Processor");
        }

        [DataRow("Hi", "Hello!", DisplayName = "Hi")]
        [DataRow("how’s life", "Everything is great! Do have a command for me?", DisplayName = "how")]
        [DataRow("nice to see you", "Thank you.", DisplayName = "nice")]
        [DataRow("Whaaaazzzup", "Hello!", DisplayName = "default")]
        [DataRow("who are you", "I am your friendly neighborhood mentorbot.", DisplayName = "who")]
        [DataRow("where are you", "I am everywhere.", DisplayName = "where")]
        [DataTestMethod]
        public async Task HelloWhenAskedShouldReturnResponse(string phrase, string expectedResult)
        {
            var result = await _processor.ProcessCommandAsync(new TextDeconstructionInformation(phrase, null), null, null, null);
            Assert.AreEqual(expectedResult, result.Text);
        }
    }
}
