using System.Linq;
using System.Runtime.ExceptionServices;
using FileQuerier.CoreLibrary;
using FileQuerier.CoreLibrary.JSON;
using NUnit.Framework;

namespace UnitTests.CoreLibrary.JSON
{
    [TestFixture]
    public class JsonToCommonFormatConverterTests
    {
        [Test]
        public void Parse_ArrayOfObjects_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("[{'firstName':'Jeppe', 'lastName':'Kristensen'}, {'firstName':'Don', 'lastName':'Draper'}]");
            BaseCheckForRoot(result, "Root","Root");
            AssertProperty(result.RootClass.Properties[0], "Contents", true);

            Assert.That(result.DependentClasses, Has.Count.EqualTo(1));
            var childClass = result.DependentClasses.First().Value;
            AssertCommonClass(childClass,"Root_Contents","Root_Contents");
            AssertProperty(childClass.Properties[0], "firstName", false);
            AssertProperty(childClass.Properties[1], "lastName", false);
        }

        [Test]
        public void Parse_BasicObject_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("{'firstName':'Jeppe', 'lastName':'Kristensen'}");
            BaseCheckForRoot(result, "Root", "Root");
            AssertProperty(result.RootClass.Properties[0], "firstName", false);
            AssertProperty(result.RootClass.Properties[1], "lastName", false);

            Assert.That(result.DependentClasses, Is.Empty);
        }
        [Test]
        public void Parse_IsComplexerObject_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("{'firstName':'Jeppe', 'lastName':'Kristensen', 'myArray':[{ 'type1':5}, {'type1':10}]}");
            BaseCheckForRoot(result, "Root", "Root");

            AssertProperty(result.RootClass.Properties[0], "firstName", false);
            AssertProperty(result.RootClass.Properties[1], "lastName", false);

            var thirdProperty = result.RootClass.Properties[2];

            AssertProperty(thirdProperty, "myArray", true);

            Assert.That(result.DependentClasses, Is.Not.Empty);
            var myArrayCommonClass = result.DependentClasses.First().Value;

            Assert.That(thirdProperty.CustomTypeId, Is.EqualTo(myArrayCommonClass.Id));

        }

        protected void BaseCheckForRoot(ParsedCommonInformation result, string rootId, string rootName)
        {
            Assert.That(result.RootClass, Is.Not.Null,"Root must be null");
            AssertCommonClass(result.RootClass, rootId, rootName);
        }
        protected void AssertCommonClass(CommonClass commonClass, string id, string name)
        {
            Assert.That(commonClass, Is.Not.Null);
            Assert.That(commonClass.Id, Is.EqualTo(id));
            Assert.That(commonClass.Name, Is.EqualTo(name));
        }

        protected void AssertProperty(CommonProperty property, string name, bool isArray)
        {
            Assert.That(property.Name, Is.EqualTo(name));
            Assert.That(property.IsArray, Is.EqualTo(isArray), "\{nameof(isArray)}");

        }
    }
}