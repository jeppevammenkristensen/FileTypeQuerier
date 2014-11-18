using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.JSON
{
    public class JsonToCommonFormatConverter
    {
        private Dictionary<string, CommonClass> classes = new Dictionary<string, CommonClass>(StringComparer.CurrentCultureIgnoreCase);
        private CommonClass Root = null;
        protected CommonClass CurrentClass
        {
            get
            {
                return ClassStack.Count == 0 ? null : ClassStack.Peek();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("\{nameof(CurrentClass)}");

                ClassStack.Push(value);
            }
        }

        protected CommonProperty CurrentProperty
        {
            get
            {
                return PropertyStack.Count == 0 ? null : PropertyStack.Peek();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("\{nameof(CommonProperty)}");

                PropertyStack.Push(value);
            }
        }

        private Stack<CommonClass> ClassStack = new Stack<CommonClass>();
        private Stack<CommonProperty> PropertyStack = new Stack<CommonProperty>();

        public ParsedCommonInformation ParseJson(string json)
        {


            using (var strreader = new StringReader(json))
            {
                using (var reader = new JsonTextReader(strreader))
                {
                    while (reader.Read())
                    {
                        var tokenType = reader.TokenType;
                        switch (tokenType)
                        {

                            case JsonToken.StartObject:
                                VisitStartObject(reader);
                                break;
                            case JsonToken.StartArray:
                                VisitStartArray(reader);
                                break;
                            case JsonToken.StartConstructor:
                                VisitStartConstructor(reader);
                                break;
                            case JsonToken.PropertyName:
                                VisitPropertyName(reader);
                                break;
                            case JsonToken.Comment:
                                VisitComment(reader);
                                break;
                            case JsonToken.EndObject:
                                VisitEndObject(reader);
                                break;
                            case JsonToken.EndArray:
                                VisitEndArray(reader);
                                break;
                            case JsonToken.Float:
                            case JsonToken.String:
                            case JsonToken.Boolean:
                            case JsonToken.Undefined:
                            case JsonToken.Date:
                            case JsonToken.Bytes:
                            case JsonToken.Integer:
                                VisitPredefinedType(reader, tokenType);
                                break;
                            case JsonToken.Null:
                                VisitNull(reader);
                                break;
                            case JsonToken.Raw:
                            default:
                                break;
                        }

                    }
                };
            }
            return new ParsedCommonInformation(classes, Root, json);
        }
        private void VisitNull(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }


        //    //var result = JsonConvert.,
        //    return ClassDeclaration("Root")
        //        .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
        //        .WithMembers(List<MemberDeclarationSyntax>(new[] {
        //                PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),"firstName")
        //                    .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
        //                    .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
        //    })).ToString();
        //}


        private void VisitPredefinedType(JsonTextReader reader, JsonToken tokenType)
        {
            if (CurrentProperty == null)
                return;

            CurrentProperty.Type = ConvertToCommonType(tokenType);

            if (!CurrentProperty.IsArray)
            {
                CurrentClass.AddProperties(PropertyStack.Pop());
            }

        }

        private CommonType ConvertToCommonType(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.Integer:
                    return CommonType.Integer;
                case JsonToken.Float:
                    return CommonType.Float;
                case JsonToken.String:
                    return CommonType.StringType;
                case JsonToken.Boolean:
                    return CommonType.Boolean;
                case JsonToken.Undefined:
                    return CommonType.Undetermined;
                case JsonToken.Date:
                    return CommonType.Date;
                case JsonToken.Bytes:
                    return CommonType.Bytes;
                default:
                    throw new InvalidOperationException("Does not allow \{nameof(token)} to be value \{token}");
            }
        }
        private void VisitStartArray(JsonTextReader reader)
        {
            if (CurrentProperty == null)
                CurrentProperty.Name = "Contents";

            CurrentProperty.IsArray = true;
        }

        private void VisitEndArray(JsonTextReader reader)
        {
            if (!CurrentProperty.IsCustomType)
                CurrentClass.AddProperties(CurrentProperty);

            PropertyStack.Pop();
        }

        private void VisitStartObject(JsonTextReader reader)
        {
            if (!classes.Any() && CurrentClass == null)
            {
                CurrentClass = new CommonClass();
                CurrentClass.Id = "Root";
                CurrentClass.Name = "Root";
            }
            else
            {
                CurrentClass = new CommonClass();
                CurrentClass.Name = CurrentProperty.Name;
                CurrentClass.Id = GenerateClassId(CurrentClass.Name);
                CurrentProperty.Type = CommonType.Custom;
                CurrentProperty.IsCustomType = true;
                CurrentProperty.CustomTypeId = CurrentProperty.Name;
            }
        }

        private string GenerateClassId(string name)
        {
            var id = ClassStack.Skip(1).First()?.Id;
            return "\{id}_\{name}";
        }

        private void VisitEndObject(JsonTextReader reader)
        {
            var candidate = ClassStack.Pop();

            if (candidate.Name == "Root")
            {
                Root = candidate;
            }
            else if (classes.ContainsKey(candidate.Id))
            {
                var existing = classes[candidate.Id];
                existing.AddProperties(candidate.Properties.ToArray());
            }
            else
            {
                classes.Add(candidate.Id, candidate);
            }

            if (CurrentClass != null)
            {
                CurrentClass.AddProperties(CurrentProperty);
            }

            if (!CurrentProperty?.IsArray ?? false)
                PropertyStack.Pop();
        }

        private void VisitComment(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }

        private void VisitPropertyName(JsonTextReader reader)
        {
            CurrentProperty = new CommonProperty();
            CurrentProperty.Name = (string)reader.Value;

        }

        private void VisitStartConstructor(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
