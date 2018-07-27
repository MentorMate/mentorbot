using MentorBot.Core.Localize;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Core.Localize
{
    [TestClass]
    [TestCategory("Core.Localize")]
    public sealed class StringLocalizerTests
    {
        [TestMethod]
        public void StringLocalizerSelfReturn()
        {
            var localizer = new StringLocalizer();
            Assert.AreEqual("A", localizer["A"]);
            Assert.AreEqual("A B", localizer["A B"]);
        }
    }
}
