using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class CognitiveServiceTests
    {
        private IMemoryCache _cache;
        private IStorageService _storageService;
        private ILanguageUnderstandingConnector _connector;
        private IList<ICommandProcessor> _commandProcessors;
        private CognitiveService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _cache = Substitute.For<IMemoryCache>();
            _storageService = Substitute.For<IStorageService>();
            _connector = Substitute.For<ILanguageUnderstandingConnector>();
            _commandProcessors = new List<ICommandProcessor>();
            _service = new CognitiveService(_cache, _storageService, _commandProcessors, _connector);
        }

        [TestMethod]
        public async Task CognitiveService_ProcessShouldReturnActiveProcessor()
        {
            var processor1 = Substitute.For<ICommandProcessor>();
            var processor2 = Substitute.For<ICommandProcessor>();
            var chatEvent = GetChatEvent("@mentorbot Get My Processor. ");
            var info = new TextDeconstructionInformation(null, "My");
            var settings = new MentorBotSettings
            {
                Processors = new[]
                {
                    new ProcessorSettings
                    {
                        Name = "Processor 1",
                        Enabled = false
                    },
                    new ProcessorSettings
                    {
                        Name = "Processor 2",
                        Enabled = true,
                        Data = new[] { new KeyValuePair<string, string>("K", "") }
                    }
                }
            };

            _commandProcessors.Add(processor1);
            _commandProcessors.Add(processor2);

            processor1.Subject.Returns("My");
            processor2.Subject.Returns("My");
            processor1.Name.Returns("Processor 1");
            processor2.Name.Returns("Processor 2");

            _connector.DeconstructAsync("Get My Processor").Returns(info);

            _cache.TryGetValue(Constants.SettingsCacheKey, out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = settings;
                    return true;
                });

            var result = await _service.ProcessAsync(chatEvent);

            Assert.AreEqual(result.TextDeconstructionInformation, info);
            Assert.AreEqual(result.CommandProcessor.Name, "Processor 2");
            Assert.AreEqual(result.Settings.Count, 1);
        }

        [TestMethod]
        public async Task CognitiveService_ProcessShouldGetSettingFromStorage()
        {
            var chatEvent = GetChatEvent("@mentorbot Test");

            _cache.TryGetValue(Constants.SettingsCacheKey, out Arg.Any<object>()).Returns(false);
            _storageService.GetSettingsAsync().Returns(new MentorBotSettings { Processors = new ProcessorSettings[0] });

            var result = await _service.ProcessAsync(chatEvent);

            Assert.IsNull(result);
            await _storageService.Received().GetSettingsAsync();
        }

        private static ChatEvent GetChatEvent(string text) =>
            new ChatEvent
            {
                Message = new ChatEventMessage
                {
                    Text = text
                }
            };
    }
}
