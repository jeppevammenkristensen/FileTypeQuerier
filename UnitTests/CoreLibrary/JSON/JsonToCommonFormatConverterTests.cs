using System.IO;
using System.Linq;
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

            Assert.That(result.DependentClasses, Has.Count.EqualTo(2));
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

            Assert.That(result.DependentClasses, Has.Count.EqualTo(1));
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

        [Test]
        public void Parse_JsonObjectWithMultiplePropertiesPointingToComplexType_ReturnsExpectedIdAndNameOfClass()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("{ 'firstProperty' : { 'firstName' : 'Jeppe'}, 'secondProperty':{'lastName' : 'Kristensen' }}");

            BaseCheckForRoot(result,"Root","Root");
            var firstClass = result.DependentClasses.First();
            var secondClass = result.DependentClasses.ElementAt(1);

            Assert.That(firstClass.Value.Id, Is.EqualTo("Root_firstProperty"));
            Assert.That(secondClass.Value.Id, Is.EqualTo("Root_secondProperty"));
            
            Assert.That(firstClass.Value.Name, Is.EqualTo("Root_firstProperty"), "Name");
            Assert.That(secondClass.Value.Name, Is.EqualTo("Root_secondProperty"), "Name");
        }


        [Test]
        public void Parse_JsonObjectWithSimpleArray_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("{ 'simpleArray' : [5,7,8] }");

            BaseCheckForRoot(result, "Root","Root");

            var firstProperty = result.RootClass.Properties[0];
            Assert.That(firstProperty.IsArray, Is.True);
            Assert.That(firstProperty.IsCustomType, Is.False);
            Assert.That(firstProperty.Type, Is.EqualTo(CommonType.Integer));
        }

        [Test]
        public void Parse_SimpleTypes_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson("{ 'intProperty' : 5, 'stringProperty' : 'string', 'floatProperty': 50.0, 'booleanProperty' : true }");

            BaseCheckForRoot(result, "Root", "Root");

            AssertSimpleProperty(result.RootClass.Properties[0],"intProperty", CommonType.Integer);
            AssertSimpleProperty(result.RootClass.Properties[1], "stringProperty", CommonType.String);
            AssertSimpleProperty(result.RootClass.Properties[2], "floatProperty", CommonType.Float);
            AssertSimpleProperty(result.RootClass.Properties[3], "booleanProperty", CommonType.Boolean);
        }

        [Test]
        public void Parse_FacebookExample_VerifyResult()
        {
            var parser = new JsonToCommonFormatConverter();
            var result = parser.ParseJson(File.ReadAllText("CoreLibrary/FileSample.json"));

            BaseCheckForRoot(result, "Root", "Root");

            AssertCommonClass(result.DependentClasses["Root_data_from"], "Root_data_from", "Root_data_from");
            AssertCommonClass(result.DependentClasses["Root_data_actions"], "Root_data_actions", "Root_data_actions");
            AssertCommonClass(result.DependentClasses["Root_data"], "Root_data", "Root_data");
        }

        protected void BaseCheckForRoot(ParsedCommonInformation result, string rootId, string rootName)
        {
            Assert.That(result.RootClass, Is.Not.Null,"Root must be null");
            AssertCommonClass(result.RootClass, rootId, rootName);
            Assert.That(result.DependentClasses.Select(x => x.Value), Has.Member(result.RootClass));
        }
        protected void AssertCommonClass(CommonClass commonClass, string id, string name)
        {
            Assert.That(commonClass, Is.Not.Null);
            Assert.That(commonClass.Id, Is.EqualTo(id));
            Assert.That(commonClass.Name, Is.EqualTo(name));
        }

        protected void AssertSimpleProperty(CommonProperty property, string name, CommonType expectedProperty)
        {
            AssertProperty(property, name, false);
            Assert.That(property.Type, Is.EqualTo(expectedProperty));
        }

        protected void AssertProperty(CommonProperty property, string name, bool isArray)
        {
            Assert.That(property.Name, Is.EqualTo(name));
            Assert.That(property.IsArray, Is.EqualTo(isArray), "\{nameof(isArray)}");

        }
    }
}