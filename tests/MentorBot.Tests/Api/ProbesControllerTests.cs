using MentorBot.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Api
{
    [TestClass]
    public class ProbesControllerTests
    {
        private ProbesController _controller;

        [TestInitialize]
        public void Init()
        {
            _controller = new ProbesController();
        }

        [TestMethod]
        public void TestGetReturnVersion()
        {
            var result = _controller.Get();

            result.As<ObjectResult>()
                  .ShouldHaveStatusCode(200)
                  .ShouldReturnValue("15.7.2");
        }
    }
}
