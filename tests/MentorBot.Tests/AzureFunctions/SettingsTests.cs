using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using NSubstitute;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public sealed class SettingsTests
    {
        [TestMethod]
        public async Task GetSettings_HappyPath_GET()
        {
            var tokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var info = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };

            tokenService.ValidateTokenAsync(Arg.Any<HttpRequest>()).Returns(info);
            storageService.GetSettingsAsync().Returns(new MentorBotSettings
            {
                Processors = new List<ProcessorSettings> {
                new ProcessorSettings { Name = "Processor 1", Enabled = true },
                new ProcessorSettings { Name = "Processor 2", Enabled = false },
            }
            });

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IAccessTokenService), tokenService));

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "GET";

            var processors = (await Queries.GetSettingsAsync(context.Request)).ToArray();

            Assert.IsNotNull(processors);
            Assert.AreEqual("Processor 1", processors[0].Name);
            Assert.IsTrue(processors[0].Enabled);
            Assert.AreEqual("Processor 2", processors[1].Name);
            Assert.IsFalse(processors[1].Enabled);
        }

        [TestMethod]
        public async Task GetSettings_HappyPath_POST()
        {
            var tokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var info = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var settingsToSave = new List<ProcessorSettings>
                {
                    new ProcessorSettings { Name = "Processor 1", Enabled = true },
                    new ProcessorSettings { Name = "Processor 2", Enabled = false },
                };

            tokenService.ValidateTokenAsync(Arg.Any<HttpRequest>()).Returns(info);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                 new ServiceDescriptor(typeof(IAccessTokenService), tokenService));

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Body = new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(settingsToSave)));

            await Commands.SaveSettingsAsync(context.Request);

            await storageService
                .Received()
                .SaveSettingsAsync(
                    Arg.Is<MentorBotSettings>(it =>
                        it.Key == "MentorBotSettings" &&
                        it.Processors[0].Name == "Processor 1" &&
                        it.Processors[0].Enabled &&
                        it.Processors[1].Name == "Processor 2" &&
                        !it.Processors[1].Enabled));
        }
    }
}
