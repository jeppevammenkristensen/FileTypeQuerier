using System;
using FileQuerier.CoreLibrary;
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

        [Test]
        public void RenameClass_InvalidClassName_ThrowsException()
        {
            var parser = new JsonToCommonFormatConverter();
            var sut = parser.ParseJson("{ 'complexType' : {'firstName':'Jeppe'}}");

            var ex = Assert.Throws<InvalidOperationException>(() => sut.RenameClass("UnknownTypeId","ComplexType"));

            Assert.That(ex.Message, Is.StringContaining("UnknownTypeId"));
        }

        [Test]
        public void RenameClass_AllowCaseInsensitiveIdRename()
        {
            var parser = new JsonToCommonFormatConverter();
            var sut = parser.ParseJson("{ 'complexType' : {'firstName':'Jeppe'}}");

            sut.RenameClass("root_COMPLEXTYPE", "RenamedComplexType");

            AssertContainsClassWithName(sut, "root_COMPLEXTYPE", "RenamedComplexType");
        }

        [Test]
        public void RenameClass_CaseSensitiveNameConflict_ThrowsException()
        {
            var parser = new JsonToCommonFormatConverter();
            var sut = parser.ParseJson("{ 'complexType' : {'firstName':'Jeppe'}, 'otherComplexType' :  {'lastName':'Kristensen'}}");

            Assert.Throws<InvalidOperationException>(() => sut.RenameClass("Root_complexType", "Root_otherComplexType"));
        }


        private void AssertContainsClassWithName(ParsedCommonInformation commonInformation, string classId, string expectedName)
        {
            Assert.That(commonInformation.DependentClasses[classId].Name, Is.EqualTo(expectedName));
        }

        
    }
}