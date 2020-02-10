using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Jira;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Issues;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="IssuesProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class IssuesProcessorTests
    {
        private IssuesProcessor _processor;
        private IJiraClient _jiraClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _jiraClient = Substitute.For<IJiraClient>();
            _processor = new IssuesProcessor(_jiraClient);
        }

        [TestMethod]
        public void IssuesProcessorSubjectCheck()
        {
            Assert.AreEqual(_processor.Subject, "Issues");
        }

        [TestMethod]
        public void IssuesProcessorNameCheck()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.Issues.IssuesProcessor");
        }

        [TestMethod]
        public async Task IssuesProcessorShouldReturnMessageWhenNoHost()
        {
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var chatEvent = CreateEvent("jhon.doe.@mail.com");

            var info = await _processor.ProcessCommandAsync(null, chatEvent, null, accessor);

            Assert.AreEqual("No jira hosts are configured!", info.Text);
        }

        [TestMethod]
        public async Task IssuesProcessorShouldReturnMessageWhenNoProjectOrStatus()
        {
            var chatEvent = CreateEvent("jhon.doe@mail.com");
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var entities = new Dictionary<string, string[]>();
            var deconstructionInfo = new TextDeconstructionInformation(null, null, SentenceTypes.Question, entities, null, 1);
            accessor.GetPluginPropertyGroup("Jira.Hosts")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "Jira.Host",
                                Value = "https://jira.com",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jira.User",
                                Value = "Bill",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jira.Token",
                                Value = "ABC123",
                            },
                        }
                    });

            var info = await _processor.ProcessCommandAsync(deconstructionInfo, chatEvent, null, accessor);

            Assert.AreEqual("If you try to get Jira issues, please provide project and status!", info.Text);
        }

        [TestMethod]
        public async Task IssuesProcessorShouldReturnJobInfo()
        {
            var chatEvent = CreateEvent("jhon.doe@mail.com");
            var accessor = Substitute.For<IPluginPropertiesAccessor>();

            accessor.GetPluginPropertyGroup("Jira.Hosts")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "Jira.Host",
                                Value = "https://jira.com",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jira.User",
                                Value = "Bill",
                            },
                            new PluginPropertyValue
                            {
                                Key = "Jira.Token",
                                Value = "ABC123",
                            },
                        }
                    });

            var result = new JiraClient.IssuesResponse
            {
                Issues = new[]
                {
                    new JiraClient.Issue
                    {
                        Id = "A",
                        Key = "B",
                        Self = "http://dummy.com/",
                        Fields = new JiraClient.IssueFields
                        {
                            Summary = "C",
                            Assignee = new JiraClient.User
                            {
                                DisplayName = "D"
                            }
                        }
                    }
                }
            };

            var entities = new Dictionary<string, string[]>
            {
                { "Project", new [] { "Proj" } },
                { "State", new [] { "In Development" } },
            };

            var deconstructionInfo = new TextDeconstructionInformation(null, null, SentenceTypes.Command, entities, null, 1);

            _jiraClient.QueryAsync("Proj", "In Development", "https://jira.com", "Bill", "ABC123").Returns(result);

            var info = await _processor.ProcessCommandAsync(deconstructionInfo, chatEvent, null, accessor);
            var widget = info.Cards[0].Sections[0].Widgets[0];

            Assert.AreEqual("B", widget.KeyValue.TopLabel);
            Assert.AreEqual("Assigned to D", widget.KeyValue.BottomLabel);
            Assert.AreEqual("C", widget.KeyValue.Content);
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
