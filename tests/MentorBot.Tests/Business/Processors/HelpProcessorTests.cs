using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class HelpProcessorTests
    {
        private HelpProcessor _processor;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _processor = new HelpProcessor(_storageService);
        }

        [TestMethod]
        public void HelpProcessorSubjectShouldBeHelp()
        {
            Assert.AreEqual(_processor.Subject, "Help");
        }

        [TestMethod]
        public void HelpProcessorNameShouldBeOk()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.HelpProcessor");
        }

        [TestMethod]
        public async Task HelpWhenAskedShouldCheckTheApi()
        {
            _storageService.GetAllPluginsAsync().Returns(
                new[]
                {
                    new Plugin
                    {
                        Examples = new [] { "1" },
                    },
                    new Plugin
                    {
                        Examples = new [] { "2" },
                    },
                    new Plugin
                    {
                        Examples = new [] { "10" },
                    },
                    new Plugin
                    {
                        Examples = new [] { "3" },
                    },
                    new Plugin
                    {
                        Examples = new [] { "4", "5" },
                    },
                });

            var result = await _processor.ProcessCommandAsync(new TextDeconstructionInformation(null, null), null, null, null);

            Assert.AreEqual(result.Cards[0].Sections[0].Widgets[0].TextParagraph.Text, "1<br />2<br />3<br />4<br />10");
        }
    }
}
