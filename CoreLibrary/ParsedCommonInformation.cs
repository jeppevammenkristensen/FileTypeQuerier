using System;
using System.Collections.Generic;
using System.Linq;
using FileQuerier.CoreLibrary.Resources.ExceptionMessages;

namespace FileQuerier.CoreLibrary
{
    public class ParsedCommonInformation
    {
        public ParsedCommonInformation(Dictionary<string, CommonClass> dependentClasses, CommonClass rootClass, string json)
        {
            if (dependentClasses == null) throw new ArgumentNullException("\{nameof(dependentClasses)}");
            if (rootClass == null) throw new ArgumentNullException("\{nameof(rootClass)}");

            DependentClasses = new Dictionary<string, CommonClass>(dependentClasses,StringComparer.InvariantCultureIgnoreCase);
            DependentClasses.Add(rootClass.Id, rootClass);
            RootClass = rootClass;
            OriginalSource = json;
        }

        internal Dictionary<string, CommonClass> DependentClasses { get; }
        public string OriginalSource { get; }
        public CommonClass RootClass { get; }

        public void RenameClass(string classId, string newName)
        {
            var classToRename = ValidateAndRetrieveClassToRename(classId, newName);
            classToRename.Name = newName;
        }

        /// <summary>
        /// This will throw an exception if the validation fails
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private CommonClass ValidateAndRetrieveClassToRename(string classId, string newName)
        {
            CommonClass result;

            if (DependentClasses.ContainsKey(classId))
                result = DependentClasses[classId];
            else
                throw new InvalidOperationException(NoClassWithIdFoundExceptionMessage(classId));
            var match = DependentClasses.Where(x => x.Value != result && x.Value.Name == newName)
                .Select(x => x.Value).FirstOrDefault();

            if (match != null)
                throw new InvalidOperationException(AClassWithThatNameAllreadyExists(match.Id, match.Name));
            return result;
        }
    }
}
