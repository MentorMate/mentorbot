using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;
using MentorBot.Tests._Base;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public class HangoutChatEventTests
    {
        private const string EmptyMsg = @"{ ""type"": ""MESSAGE"", ""eventTime"": ""2019-01-14T15:55:20.120287Z"", ""token"": ""A"", ""message"": { ""sender"": { ""name"": ""users/1"", ""displayName"": ""Jhon Doe"", ""email"": ""a.b@c.com"", ""type"": ""HUMAN"" }, ""text"": """" }, ""user"": { ""name"": ""users/1"", ""displayName"": ""A"", ""avatarUrl"": null, ""email"": ""a@b.com"", ""type"": ""HUMAN"" }, ""space"":{ ""name"": ""spaces/c"", ""type"": ""DM"" } }";
        private const string FullMsg = @"{ ""type"": ""MESSAGE"", ""eventTime"": ""2019-01-14T15:55:20.120287Z"", ""token"": ""AAA"", ""message"": { ""name"": ""spaces/q/messages/y"", ""sender"": { ""name"": ""users/1"", ""displayName"": ""Jhon Doe"", ""avatarUrl"": null, ""email"": ""a.b@c.com"", ""type"": ""HUMAN"" }, ""createTime"": ""2019-01-14T15:55:20.120287Z"", ""text"": ""What is this?"", ""thread"": { ""name"": ""spaces/q/threads/y"", ""retentionSettings"": { ""state"": ""PERMANENT"" } }, ""space"": { ""name"": ""spaces/q"", ""type"": ""DM"" }, ""argumentText"": ""What is this"" }, ""user"": { ""name"": ""users/1"", ""displayName"": ""Jhon Doe"", ""avatarUrl"": null, ""email"": ""a.b@c.com"", ""type"": ""HUMAN"" }, ""space"": { ""name"": ""spaces/q"", ""type"": ""DM"" }}";

        [TestMethod]
        public async Task RunShouldCheckToken()
        {
            var context = BuildTestCase();
            var message = MockFunction.GetRequest(EmptyMsg, context);

            var result = await HangoutChatEvent.RunAsync(message, context);

            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [TestMethod]
        public async Task RunShouldCallService()
        {
            var message = new Message { Output = new ChatEventResult("OK") };
            var context = BuildTestCase();
            var requestMessage = MockFunction.GetRequest(FullMsg, context);

            context.Get<IHangoutsChatService>()
                .BasicAsync(Arg.Is<ChatEvent>(it =>
                    it.Message.Text == "What is this?" &&
                    it.Message.Name == "spaces/q/messages/y"))
                .Returns(message);

            var result = await HangoutChatEvent.RunAsync(requestMessage, context);
            var resultOutput = GetStringResult(result);

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("{\"text\":\"OK\"}", resultOutput);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task RunShouldSaveInDb()
        {
            var message = new Message();
            var context = BuildTestCase();
            var requestMessage = MockFunction.GetRequest(FullMsg, context);

            context.Get<IHangoutsChatService>().BasicAsync(null).ReturnsForAnyArgs(message);

            await HangoutChatEvent.RunAsync(requestMessage, context);

            context.Get<IStorageService>().Received().SaveMessageAsync(message);
        }

#pragma warning restore CS4014

        private FunctionContext BuildTestCase()
        {
            var storageService = Substitute.For<IStorageService>();
            var hangoutsChatService = Substitute.For<IHangoutsChatService>();
            var options = new GoogleCloudOptions("AAA", "B", "C", "D");
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IHangoutsChatService), hangoutsChatService),
                new ServiceDescriptor(typeof(GoogleCloudOptions), options));

            return context;
        }

        private static string GetStringResult(HttpResponseData data)
        {
            var count = data.Body.Length;
            if (count > 0)
            {
                byte[] byteData = new byte[count];
                data.Body.Position = 0;
                data.Body.Read(byteData, 0, (int)count);
                return Encoding.UTF8.GetString(byteData);
            }

            return null;
        }
    }
}
