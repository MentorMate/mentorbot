using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Processors.BuildInfo;

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

        [TestInitialize]
        public void TestInitialize()
        {
            _jenkinsClient = Substitute.For<IJenkinsClient>();
            _processor = new BuildInfoProcessor(_jenkinsClient);
        }

        [TestMethod]
        public void BuildInfoProcessorSubjectShoudBeBuildInfo()
        {
            Assert.AreEqual(_processor.Subject, "BuildInfo");
        }

        [TestMethod]
        public void BuildInfoProcessorNameCheck()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.BuildInfo.BuildInfoProcessor");
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnMessageWhenNoHost()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var chatEvent = CreateEvent("jhon.doe.@mail.com");
            
            accessor.GetAllUserPropertyValuesAsync<string>(null).ReturnsForAnyArgs(new[] { "property_value" });
            
            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, accessor);

            Assert.AreEqual("No jenkins hosts are configured!", info.Text);
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnMessageWhenUserDoNotHaveJobs()
        {
            var chatEvent = CreateEvent("jhon.doe@mail.com");
            var accessor = Substitute.For<IPluginPropertiesAccessor>();

            //accessor.GetAllUserPropertyValuesAsync<string>(null).ReturnsForAnyArgs(new string[0]);

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, accessor);

            Assert.AreEqual("No jobs are assign to you!", info.Text);
        }

        [TestMethod]
        public async Task BuildInfoProcessorShouldReturnJobInfo()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();

            accessor.GetAllUserPropertyValuesAsync<string>(null).ReturnsForAnyArgs(new[] { "TestJob" });
            accessor.GetPluginPropertyGroup("Jenkins.Hosts")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "Jenkins.Host",
                                Value = "https://jenkins.com",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jenkins.User",
                                Value = "Bill",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jenkins.Token",
                                Value = "ABC123",
                            },
                        }
                    });

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

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, accessor);
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
