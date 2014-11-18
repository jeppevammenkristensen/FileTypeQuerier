using System.Collections.Generic;

namespace CoreLibrary
{
    public class ParsedCommonInformation
    {
        public ParsedCommonInformation(Dictionary<string, CommonClass> dependentClasses, CommonClass rootClass, string json)
        {
            DependentClasses = dependentClasses;
            RootClass = rootClass;
            OriginalSource = json;
        }

        public Dictionary<string, CommonClass> DependentClasses { get; }
        public string OriginalSource { get; }
        public CommonClass RootClass { get; }
    }
}
