using FileQuerier.CoreLibrary.JSON;
using NUnit.Framework;

namespace UnitTests.CoreLibrary
{
    [TestFixture]
    public class ParsedCommonInformationTester
    {
        [Test]
        public void RenameClass_ValidClassName_RenamesClassName()
        {
            var parser = new JsonToCommonFormatConverter();
            var sut = parser.ParseJson("{ 'complexType' : {'firstName':'Jeppe'}}");

            sut.RenameClass("Root_complexType", "ComplexType");

            Assert.That(sut.DependentClasses["Root_complexType"].Name, Is.EqualTo("ComplexType"));
        }
    }
}