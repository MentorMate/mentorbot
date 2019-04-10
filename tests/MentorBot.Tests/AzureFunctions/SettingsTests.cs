using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.AzureFunctions;
using MentorBot.Functions.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public sealed class SettingsTests
    {
        [TestMethod]
        public async Task GetSettings_HappyPath_GET()
        {
            var storageService = Substitute.For<IStorageService>();

            storageService.GetSettingsAsync().Returns(new MentorBotSettings { Processors = new List<ProcessorSettings> {
                new ProcessorSettings { Name = "Processor 1", Enabled = true },
                new ProcessorSettings { Name = "Processor 2", Enabled = false },
            } });

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService));

            Microsoft.Extensions.Logging.ILogger logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "GET";

            var result = await Settings.Run(context.Request, logger) ;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNotNull((result as OkObjectResult).Value);
            Assert.IsInstanceOfType((result as OkObjectResult).Value, typeof(MentorBotSettings));

            var settings = (result as OkObjectResult).Value as MentorBotSettings;
            Assert.AreEqual("MentorBotSettings", settings.Key);
            Assert.IsNotNull(settings.Processors);
            Assert.AreEqual("Processor 1", settings.Processors[0].Name);
            Assert.IsTrue(settings.Processors[0].Enabled);
            Assert.AreEqual("Processor 2", settings.Processors[1].Name);
            Assert.IsFalse(settings.Processors[1].Enabled);
        }

        [TestMethod]
        public async Task GetSettings_HappyPath_POST()
        {
            var storageService = Substitute.For<IStorageService>();
            var settingsToSave = new MentorBotSettings
            {
                Processors = new List<ProcessorSettings>
                {
                    new ProcessorSettings { Name = "Processor 1", Enabled = true },
                    new ProcessorSettings { Name = "Processor 2", Enabled = false },
                }
            };
            storageService.SaveSettingsAsync(Arg.Is<MentorBotSettings>(s => 
                s.Processors != null && 
                s.Processors.Count == 2 &&
                s.Processors[0].Name == "Processor 1" && s.Processors[0].Enabled == true &&
                s.Processors[1].Name == "Processor 2" && s.Processors[1].Enabled == false)).Returns(true);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService));

            Microsoft.Extensions.Logging.ILogger logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Body = new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(settingsToSave)));

            var result = await Settings.Run(context.Request, logger) ;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNotNull((result as OkObjectResult).Value);
            Assert.IsInstanceOfType((result as OkObjectResult).Value, typeof(MentorBotSettings));

            var settings = (result as OkObjectResult).Value as MentorBotSettings;
            Assert.AreEqual("MentorBotSettings", settings.Key);
            Assert.IsNotNull(settings.Processors);
            Assert.AreEqual("Processor 1", settings.Processors[0].Name);
            Assert.IsTrue(settings.Processors[0].Enabled);
            Assert.AreEqual("Processor 2", settings.Processors[1].Name);
            Assert.IsFalse(settings.Processors[1].Enabled);
        }

        [TestMethod]
        public async Task GetSettings_BadRequest()
        {
            Microsoft.Extensions.Logging.ILogger logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "PUT";

            var result = await Settings.Run(context.Request, logger) ;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Unsupported method", (result as BadRequestObjectResult).Value);
        }
    }
}
