using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class PluginPropertiesAccessorTests
    {
        private PluginPropertiesAccessor _accessor;

        [TestMethod]
        public void PluginAccessorWithPluginWithNoGeupShoudStillReturn()
        {
            _accessor = PluginPropertiesAccessor.GetInstance(null, new Plugin(), null);
            Assert.IsNotNull(_accessor.GetAllPluginPropertyValues<string>("Test"));
            Assert.IsNotNull(_accessor.GetPluginPropertyGroup("Test2"));
        }

        [TestMethod]
        public void PluginAccessorShouldGetPluginValues()
        {
            var plugin = new Plugin
            {
                Groups = new[]
                {
                    new PluginPropertyGroup
                    {
                        Values = GetGroup(
                                new PluginPropertyValue
                                {
                                    Key = "Test3",
                                    Value = "a"
                                },
                                new PluginPropertyValue
                                {
                                    Key = "Test3",
                                    Value = "b"
                                })
                    }
                }
            };

            _accessor = PluginPropertiesAccessor.GetInstance(null, plugin, null);
            Assert.AreEqual(2, _accessor.GetAllPluginPropertyValues<string>("Test3").Count);
        }

        [TestMethod]
        public void PluginAccessorShouldGetPluginValueIfNotCorrectType()
        {
            var plugin = new Plugin
            {
                Groups = new[]
                {
                    new PluginPropertyGroup
                    {
                        Values = GetGroup(
                                new PluginPropertyValue
                                {
                                    Key = "TestBool",
                                    Value = string.Empty
                                },
                                new PluginPropertyValue
                                {
                                    Key = "TestStrBool",
                                    Value = "True"
                                })
                    }
                }
            };

            _accessor = PluginPropertiesAccessor.GetInstance(null, plugin, null);
            Assert.IsFalse(_accessor.GetAllPluginPropertyValues<bool>("TestBool").First());
            Assert.IsTrue(_accessor.GetAllPluginPropertyValues<bool>("TestStrBool").First());
        }

        [TestMethod]
        public void PluginAccessorShouldGetPluginGroup()
        {
            var value1 = new PluginPropertyValue();
            var value2 = new PluginPropertyValue();
            var group1 = new PluginPropertyGroup { UniqueName = "G1", Values = GetGroup(value1) };
            var group2 = new PluginPropertyGroup { UniqueName = "G2", Values = GetGroup(value2) };
            var plugin = new Plugin { Groups = new[] { group1, group2 } };
            _accessor = PluginPropertiesAccessor.GetInstance(null, plugin, null);

            var values = _accessor.GetPluginPropertyGroup("G2");
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(value2, values[0][0]);
        }

        [TestMethod]
        public async Task PluginAccessorShouldGetUserProperty()
        {
            var user = new User
            {
                Properties = new Dictionary<string, PluginPropertyValue[][]>
                {
                    { "A", GetGroup(new PluginPropertyValue { Key = "Test3", Value = "a" }) },
                    { "B", GetGroup(new PluginPropertyValue { Key = "Test3", Value = "b" }) },
                }
            };

            var storageService = Substitute.For<IStorageService>();
            _accessor = PluginPropertiesAccessor.GetInstance("r@d.c", null, storageService);

            storageService.GetUserByEmailAsync("r@d.c").Returns(user);

            var result = await _accessor.GetAllUserPropertyValuesAsync<string>("Test3");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void PluginAccessorShouldShouldGetAllPluginValuesIfSomeIsNull()
        {
            var value1 = new PluginPropertyValue { Key = "V1", Value = 2 };
            var group1 = new PluginPropertyGroup { UniqueName = "G1", Values = GetGroup(value1) };
            var group2 = new PluginPropertyGroup { UniqueName = "G2", Values = null };
            var plugin = new Plugin { Groups = new[] { group1, group2 } };
            _accessor = PluginPropertiesAccessor.GetInstance(null, plugin, null);

            var result = _accessor.GetAllPluginPropertyValues<int>("V1");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0]);
        }

        private static PluginPropertyValue[][] GetGroup(params PluginPropertyValue[] values) =>
            new[] { values };
    }
}
