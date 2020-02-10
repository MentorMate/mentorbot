using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
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
            var settings = new Plugin[]
            {
                new Plugin
                {
                    ProcessorTypeName = "Processor 1",
                    Enabled = false,
                },
                new Plugin
                {
                    ProcessorTypeName = "Processor 2",
                    Enabled = true,
                }
            };

            _commandProcessors.Add(processor1);
            _commandProcessors.Add(processor2);

            processor1.Subject.Returns("My");
            processor2.Subject.Returns("My");
            processor1.Name.Returns("Processor 1");
            processor2.Name.Returns("Processor 2");

            _connector.DeconstructAsync("Get My Processor").Returns(info);

            _cache.TryGetValue(Constants.PluginsCacheKey, out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = settings;
                    return true;
                });

            var result = await _service.ProcessAsync(chatEvent);

            Assert.AreEqual(result.TextDeconstructionInformation, info);
            Assert.AreEqual(result.CommandProcessor.Name, "Processor 2");
            Assert.IsNotNull(result.PropertiesAccessor);
        }

        [TestMethod]
        public async Task CognitiveService_ProcessShouldGetPluginsFromStorage()
        {
            var chatEvent = GetChatEvent("@mentorbot Test");

            _cache.TryGetValue(Constants.PluginsCacheKey, out Arg.Any<object>()).Returns(false);
            _storageService
                .GetAllPluginsAsync()
                .Returns(
                    new Plugin[]
                    {
                        new Plugin()
                    });

            var result = await _service.ProcessAsync(chatEvent);

            Assert.IsNull(result);
            await _storageService.Received().GetAllPluginsAsync();
        }

        [TestMethod]
        public async Task CognitiveService_ProcessShouldInitializeAccessor()
        {
            var processor = Substitute.For<ICommandProcessor>();
            _cache.TryGetValue(Constants.PluginsCacheKey, out Arg.Any<object>()).Returns(false);
            _commandProcessors.Add(processor);
            _storageService
                .GetAllPluginsAsync()
                .Returns(
                    new Plugin[]
                    {
                        new Plugin
                        {
                            ProcessorTypeName = "Processorrrrr",
                            Enabled = true,
                            Groups = new []
                            {
                                new PluginPropertyGroup
                                {
                                    Name = "Grouppp",
                                    UniqueName = "G1",
                                    Values = new []
                                    {
                                        new []
                                        {
                                            new PluginPropertyValue
                                            {
                                                Key = "P1",
                                                Value = "Apaplda"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

            processor.Subject.Returns("Subj");
            processor.Name.Returns("Processorrrrr");

            var result = await _service.GetCognitiveTextAnalysisResultAsync(
                new TextDeconstructionInformation("Test", "Subj"), "dummy@domain.com");

            var group = result.PropertiesAccessor.GetPluginPropertyGroup("G1");
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual("Apaplda", group[0][0].Value);
            Assert.AreEqual(processor, result.CommandProcessor);
        }

        private static ChatEvent GetChatEvent(string text) =>
            new ChatEvent
            {
                Message = new ChatEventMessage
                {
                    Sender = new ChatEventMessageSender
                    {
                        Email = "a@b.c"
                    },
                    Text = text
                }
            };
    }
}
