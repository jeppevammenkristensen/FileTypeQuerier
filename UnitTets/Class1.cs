using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTets
{
    [TestFixture]
    public class ConverterTests
    {
        [Test]
        public void ConvertJson_SingleObject_ToExpectedClass()
        {
            var name = "Jeppe";
            string.Format("{name} er sej");


            var src = "{'firstName':'Jeppe','lastName':'Kristensen'}";
         }
    }
}
