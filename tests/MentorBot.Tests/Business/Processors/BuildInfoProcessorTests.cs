using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class BuildInfoProcessorTests
    {
        private BuildInfoProcessor _processor;
        private IJenkinsClient _jenkinsClient;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _jenkinsClient = Substitute.For<IJenkinsClient>();
            _storageService = Substitute.For<IStorageService>();
            _processor = new BuildInfoProcessor(_jenkinsClient, _storageService);
        }

        [TestMethod]
        public void BuildInfoProcessorSubjectShoudBeBuildInfo()
        {
            Assert.AreEqual(_processor.Subject, "BuildInfo");
        }

        [TestMethod]
        public void BuildInfoProcessorNameCheck()
        {
            Assert.AreEqual(_processor.Name, "Build Info Processor");
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnMessageWhenNoUser()
        {
            var chatEvent = CreateEvent("jhon.doe.@mail.com");
            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, null);
            Assert.AreEqual("No jobs are assign to you!", info.Text);
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnMessageWhenUserDoNotHaveProperties()
        {
            var chatEvent = CreateEvent("jhon.doe@mail.com");
            _storageService.GetUserByEmailAsync("jhon.doe@mail.com").Returns(new User());

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, null);

            Assert.AreEqual("No jobs are assign to you!", info.Text);
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnMessageWhenUserDoNotHaveJobs()
        {
            var user = new User
            {
                Properties = new Dictionary<string, IReadOnlyList<object>>
                {
                    { "Test", new object[0] }
                }
            };

            var chatEvent = CreateEvent("jhon.doe@mail.com");
            _storageService.GetUserByEmailAsync("jhon.doe@mail.com").Returns(user);

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, null);

            Assert.AreEqual("No jobs are assign to you!", info.Text);
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnJobInfo()
        {
            var user = new User
            {
                Properties = new Dictionary<string, IReadOnlyList<object>>
                {
                    { "JenkinsJobs", new [] { "TestJob" } }
                }
            };

            var settings = new Dictionary<string, string>
            {
                { "Host", "https://jenkins.com" },
                { "Username", "Bill" },
                { "Token", "ABC123" },
            };

            var result = new JenkinsClient.JobResponse
            {
                DisplayName = "Job 1",
                Description = "Job 1 Desc",
                Building = true,
                Result = "RUNNING",
                Url = "https://jenkins.com/job/1",
                ChangeSet = new JenkinsClient.ChangeSet
                {
                    Items = new[]
                    {
                        new JenkinsClient.ChangeSetItem
                        {
                            Comment = "Job Comment"
                        }
                    }
                }
            };

            var chatEvent = CreateEvent("jhon.doe@mail.com");
            _jenkinsClient.QueryAsync("TestJob", "https://jenkins.com", "Bill", "ABC123").Returns(result);
            _storageService.GetUserByEmailAsync("jhon.doe@mail.com").Returns(user);

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, settings);
            var widget = info.Cards[0].Sections[0].Widgets[0];

            Assert.AreEqual("Job 1", widget.KeyValue.TopLabel);
            Assert.AreEqual("RUNNING", widget.KeyValue.BottomLabel);
            Assert.AreEqual("Job Comment", widget.KeyValue.Content);
        }

        private static ChatEvent CreateEvent(string senderEmail)
        {
            var sender = new ChatEventMessageSender() { Email = senderEmail };
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            return new ChatEvent { Space = space, Message = message };
        }
    }
}
