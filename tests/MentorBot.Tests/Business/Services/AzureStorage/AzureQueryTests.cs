using System.Linq;

using CoreHelpers.WindowsAzure.Storage.Table.Models;

using MentorBot.Functions.Services.AzureStorage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Services.AzureStorage
{
    [TestClass]
    [TestCategory("Business.Services.Azure")]
    public sealed class AzureQueryTests
    {
        [TestMethod]
        public void AzureQueryShouldCreateWhereExp()
        {
            var result = AzureQuery.CreateQueryFilters("Prop1 eq 1 AND Prop2 ne 2").ToArray();

            Assert.AreEqual(result[0].FilterType, QueryFilterType.Where);
            Assert.AreEqual(result[0].Operator, QueryFilterOperator.Equal);
            Assert.AreEqual(result[0].Property, "Prop1");
            Assert.AreEqual(result[0].Value, "1");
            Assert.AreEqual(result[1].FilterType, QueryFilterType.And);
            Assert.AreEqual(result[1].Operator, QueryFilterOperator.NotEqual);
            Assert.AreEqual(result[1].Property, "Prop2");
            Assert.AreEqual(result[1].Value, "2");
        }
    }
}
