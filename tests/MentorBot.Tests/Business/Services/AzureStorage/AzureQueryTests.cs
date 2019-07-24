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

        [TestMethod]
        public void AzureQueryShouldCreateOrExp()
        {
            var result = AzureQuery.CreateQueryFilters("A < 1 OR B > 2 OR C <= 3 OR D >= 4").ToArray();

            Assert.AreEqual(result[0].FilterType, QueryFilterType.Where);
            Assert.AreEqual(result[0].Operator, QueryFilterOperator.Lower);
            Assert.AreEqual(result[0].Property, "A");
            Assert.AreEqual(result[0].Value, "1");
            Assert.AreEqual(result[1].FilterType, QueryFilterType.Or);
            Assert.AreEqual(result[1].Operator, QueryFilterOperator.Greater);
            Assert.AreEqual(result[1].Property, "B");
            Assert.AreEqual(result[1].Value, "2");
            Assert.AreEqual(result[2].FilterType, QueryFilterType.Or);
            Assert.AreEqual(result[2].Operator, QueryFilterOperator.LowerEqual);
            Assert.AreEqual(result[2].Property, "C");
            Assert.AreEqual(result[2].Value, "3");
            Assert.AreEqual(result[3].FilterType, QueryFilterType.Or);
            Assert.AreEqual(result[3].Operator, QueryFilterOperator.GreaterEqual);
            Assert.AreEqual(result[3].Property, "D");
            Assert.AreEqual(result[3].Value, "4");
        }
    }
}
