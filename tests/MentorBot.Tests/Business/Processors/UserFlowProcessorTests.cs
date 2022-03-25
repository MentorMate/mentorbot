using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
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
        public async Task WhenInactiveStateUserFlowShouldReturnMentorTypesAndActivateUserState()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var userEmail = "rosen.kolev@mentormate.com";

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

            var result = await _processor.ProcessCommandAsync(null, null, null, accessor);

            var expectedValues = new string[]
            {
                "1.Office",
                "2.Flexible Remote",
                "3.Contractor",
                "4.Dev Campers -> QA",
            };

            var stateAfterStartingUserFlow = await _storageService.GetStateAsync(userEmail);

            Assert.AreEqual("Select your Mentor Mater Type.", result.Cards[0].Header.Title);

            for (int i = 0; i < expectedValues.Length; i++)
            {
                Assert.AreEqual(expectedValues[i], result.Cards[0].Sections[0].Widgets[i].TextParagraph.Text);
            }

            Assert.AreEqual(stateAfterStartingUserFlow.Active, true);
        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldSetStateToInactive()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var userEmail = "rosen.kolev@mentormate.com";
            var parentId = "1";
            var info = new TextDeconstructionInformation("2", null);

            _storageService.GetStateAsync(userEmail).Returns(
                new State
                {
                    UserEmail = userEmail,
                    Active = true,
                    CurrentQuestionId = parentId,
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

           var result = await _processor.ProcessCommandAsync(info, null, null, accessor);

            var stateAfterReceivedAnswer = _storageService.GetStateAsync(userEmail);

            Assert.AreEqual(stateAfterReceivedAnswer, false);
        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldEmptySetStateQuestions()
        {

        }

        [TestMethod]
        public async Task ChoosingFinalQuestionShouldReturnAnswer()
        {

        }
    }
}
