using System;

namespace FileQuerier.CoreLibrary
{
    public class CommonProperty
    {
        public bool IsArray { get; set; }
        public CommonType Type { get; set; }
        public string Name { get; set; }
        public bool IsCustomType { get; internal set; }
        public string CustomTypeId { get; internal set; }

        public override bool Equals(object obj)
        {
            var parsed = obj as CommonProperty;
            if (parsed == null)
            {
                return false;
            }

            return Equals(parsed);
        }

        public bool Equals(CommonProperty p)
        {
            return string.Equals(p.Name, Name, StringComparison.InvariantCultureIgnoreCase) && p.Type == Type;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }
}
