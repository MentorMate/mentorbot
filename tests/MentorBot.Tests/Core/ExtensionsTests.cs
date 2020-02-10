using System.Net.Http;

using MentorBot.Functions.App.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Core
{
    [TestClass]
    [TestCategory("Core")]
    public sealed class ExtensionsTests
    {
        [TestMethod]
        public void HttpRequestHeadersBasicAuthentication()
        {
            var headers = new HttpClient().DefaultRequestHeaders;
            headers.BasicAuthentication("RR", "AS");
            Assert.AreEqual("Basic", headers.Authorization.Scheme);
            Assert.AreEqual("UlI6QVM=", headers.Authorization.Parameter);
        }
    }
}
