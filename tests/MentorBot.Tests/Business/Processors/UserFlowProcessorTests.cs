using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.UserFlow;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class UserFlowProcessorTests
    {
        private UserFlowProcessor _processor;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _processor = new UserFlowProcessor(_storageService);
        }

        [TestMethod]
        public void UserInfoProcessorSubjectShouldBeUser()
        {
            Assert.AreEqual(_processor.Subject, "Userflow");
        }

        [TestMethod]
        public void NameShouldBeOk()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.UserFlow.UserFlowProcessor");
        }

        [TestMethod]
        public async Task WhenInactiveStateUserFlowShouldActivateUserStateAndAskForMentorMaterType()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var userEmail = "rosen.kolev@mentormate.com";
            var info = new TextDeconstructionInformation("", null);
            var chatEvent = GetChatEvent("@mentorbot Get My Processor. ");


            _storageService.GetStateAsync(userEmail).Returns(
                new State
                {
                    UserEmail = userEmail,
                    Active = false,
                });

            accessor.GetPluginPropertyGroup("UserFlow.Hosts")
            .Returns(
                new[]
                {
                    new []
                    {
                        new PluginPropertyValue
                        {
                            Key = "UserFlow.User",
                            Value = userEmail,
                        },
                    }
                });

            var result = await _processor.ProcessCommandAsync(info, chatEvent, null, accessor);

            var stateAfterStartingUserFlow = await _storageService.GetStateAsync(userEmail);

            Assert.AreEqual("Select your Mentor Mater Type.", result.Cards[0].Header.Title);

            Assert.AreEqual(true, stateAfterStartingUserFlow.Active);
        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldSetStateToInactive()
        {
            var userEmail = "rosen.kolev@mentormate.com";

            await FinalQuestionPreparation(userEmail);

            var stateAfterReceivedAnswer = await _storageService.GetStateAsync(userEmail);

            Assert.AreEqual(false, stateAfterReceivedAnswer.Active);
        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldEmptySetStateQuestions()
        {
            var userEmail = "rosen.kolev@mentormate.com";

            await FinalQuestionPreparation(userEmail);

            var stateAfterReceivedAnswer = await _storageService.GetStateAsync(userEmail);

            Assert.AreEqual(0, stateAfterReceivedAnswer.Traits.Count);
        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldReturnAnswer()
        {
            var userEmail = "rosen.kolev@mentormate.com";

            var result = await FinalQuestionPreparation(userEmail);

            Assert.AreEqual("answer title", result.Cards[0].Header.Title);
        }

        private async Task<ChatEventResult> FinalQuestionPreparation(string userEmail)
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var parentId = "1";
            var info = new TextDeconstructionInformation("1", null);
            var chatEvent = GetChatEvent("1");
            var testTrait = "test trait";

            _storageService.GetStateAsync(userEmail).Returns(
                new State
                {
                    UserEmail = userEmail,
                    Active = true,
                });

            _storageService.GetAllQuestionsAsync().Returns(
                new List<QuestionAnswer>()
                {
                    new QuestionAnswer() { Id = "1", IsAnswer = false, AcquireTraits = new string[] { testTrait } },
                    new QuestionAnswer() { Id = "10", Parents =
                    new Dictionary<string, string>()
                    { { parentId, "parent" } }, IsAnswer = true, RequiredTraits = new string[]{ testTrait }, Title = "answer title" }
                });

            var result = await _processor.ProcessCommandAsync(info, chatEvent, null, accessor);
            return result;
        }

        private static ChatEvent GetChatEvent(string text) =>
            new ChatEvent
            {
                Message = new ChatEventMessage
                {
                    Sender = new ChatEventMessageSender
                    {
                        Email = "rosen.kolev@mentormate.com"
                    },
                    Text = text
                }
            };
    }
}
