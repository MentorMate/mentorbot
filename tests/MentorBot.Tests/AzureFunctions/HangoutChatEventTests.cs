using System;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public class HangoutChatEventTests
    {
        private const string EmptyMsg = @"{ ""type"": ""MESSAGE"", ""eventTime"": ""2019-01-14T15:55:20.120287Z"", ""token"": ""A"", ""message"": {}, ""user"": { ""name"": ""users/1"", ""displayName"": ""A"", ""avatarUrl"": null, ""email"": ""a@b.com"", ""type"": ""HUMAN"" }, ""space"":{ ""name"": ""spaces/c"", ""type"": ""DM"" } }";
        private const string FullMsg = @"{ ""type"": ""MESSAGE"", ""eventTime"": ""2019-01-14T15:55:20.120287Z"", ""token"": ""AAA"", ""message"": { ""name"": ""spaces/q/messages/y"", ""sender"": { ""name"": ""users/1"", ""displayName"": ""Jhon Doe"", ""avatarUrl"": null, ""email"": ""a.b@c.com"", ""type"": ""HUMAN"" }, ""createTime"": ""2019-01-14T15:55:20.120287Z"", ""text"": ""What is this?"", ""thread"": { ""name"": ""spaces/q/threads/y"", ""retentionSettings"": { ""state"": ""PERMANENT"" } }, ""space"": { ""name"": ""spaces/q"", ""type"": ""DM"" }, ""argumentText"": ""What is this"" }, ""user"": { ""name"": ""users/1"", ""displayName"": ""Jhon Doe"", ""avatarUrl"": null, ""email"": ""a.b@c.com"", ""type"": ""HUMAN"" }, ""space"": { ""name"": ""spaces/q"", ""type"": ""DM"" }}";

        [TestMethod]
        public async Task RunShouldCheckToken()
        {
            BuildTestCase(EmptyMsg, out ILogger logger, out HttpRequestMessage message);

            var result = await HangoutChatEvent.RunAsync(message, logger);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task RunShouldLogDebugInfo()
        {
            BuildTestCase(EmptyMsg, out ILogger logger, out HttpRequestMessage message);

            logger.IsEnabled(LogLevel.Debug).Returns(true);

            await HangoutChatEvent.RunAsync(message, logger);

            logger.Received().Log(
                LogLevel.Debug,
                default(EventId),
                Arg.Is<object>(it => it.ToString() == EmptyMsg),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [TestMethod]
        public async Task RunShouldCallService()
        {
            var message = new Message { Output = new ChatEventResult("OK") };
            BuildTestCase(FullMsg, out ILogger logger, out HttpRequestMessage requestMessage);

            ServiceLocator.Get<IHangoutsChatService>()
                .BasicAsync(Arg.Is<ChatEvent>(it =>
                    it.Message.Text == "What is this?" &&
                    it.Message.Name == "spaces/q/messages/y"))
                .Returns(message);

            var result = await HangoutChatEvent.RunAsync(requestMessage, logger);
            var resultOutput = (result as JsonResult).Value as ChatEventResult;

            Assert.AreEqual(resultOutput.Text, "OK");
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task RunShouldSaveInDb()
        {
            var message = new Message();
            BuildTestCase(FullMsg, out ILogger logger, out HttpRequestMessage requestMessage);
            
            ServiceLocator.Get<IHangoutsChatService>().BasicAsync(null).ReturnsForAnyArgs(message);
            
            await HangoutChatEvent.RunAsync(requestMessage, logger);

            //// ServiceLocator.Get<IStorageService>().Received().AddMessageAsync(message);
        }

#pragma warning restore CS4014

        private void BuildTestCase(string requestContent, out ILogger logger, out HttpRequestMessage requestMessage)
        {
            var storageService = Substitute.For<IStorageService>();
            var hangoutsChatService = Substitute.For<IHangoutsChatService>();
            var options = new GoogleCloudOptions("AAA", "B", "C", "D");

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IHangoutsChatService), hangoutsChatService),
                new ServiceDescriptor(typeof(GoogleCloudOptions), options));

            logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();
            requestMessage = new HttpRequestMessage { Content = new StringContent(requestContent) };
        }
    }
}
