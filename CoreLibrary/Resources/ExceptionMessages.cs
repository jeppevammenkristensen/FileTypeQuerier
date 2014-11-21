namespace FileQuerier.CoreLibrary.Resources
{
    
    internal static class ExceptionMessages
    {
        internal static string NoClassWithIdFoundExceptionMessage(string classId) => "No class with id \{classId}was found";
        internal static string AClassWithThatNameAllreadyExists(string conflictId, string nameConflict) => "A class with name \{nameConflict} it's id is \{conflictId}";
    }
}