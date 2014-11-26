using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Newtonsoft.Json;

namespace FileQuerier.CoreLibrary.JSON
{
    public class JsonToCommonFormatConverter
    {
        private readonly Dictionary<string, CommonClass> classes = new Dictionary<string, CommonClass>(StringComparer.CurrentCultureIgnoreCase);
        private CommonClass _root = null;
        private readonly Stack<CurrentArrayState> _arrayState = new Stack<CurrentArrayState>();
        private readonly Stack<CommonClass> _classStack = new Stack<CommonClass>();
        private readonly Stack<CommonProperty> _propertyStack = new Stack<CommonProperty>();

        protected CommonClass CurrentClass
        {
            get
            {
                return _classStack.Count == 0 ? null : _classStack.Peek();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("\{nameof(CurrentClass)}");

                _classStack.Push(value);
            }
        }

        protected CommonProperty CurrentProperty
        {
            get
            {
                return _propertyStack.Count == 0 ? null : _propertyStack.Peek();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("\{nameof(CommonProperty)}");

                _propertyStack.Push(value);
            }
        }

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

            WrapUp();

            return new ParsedCommonInformation(classes, _root, json);
        }

        private void WrapUp()
        {
            if (_root == null && _classStack.Count == 1)
            {
                _root = _classStack.Pop();
            }
        }

        private void VisitNull(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }

        private void VisitPredefinedType(JsonTextReader reader, JsonToken tokenType)
        {
            if (CurrentProperty == null)
                return;

            CurrentProperty.Type = ConvertToCommonType(tokenType);

            if (!CurrentProperty.IsArray)
            {
                CurrentClass.AddProperties(_propertyStack.Pop());
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
                    return CommonType.String;
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
            _arrayState.Push(new CurrentArrayState());

            if (CurrentProperty == null)
            {
                CurrentProperty = new CommonProperty {Name = "Contents"};
            }

            if (CreateRootClass)
            {
                CurrentClass = new CommonClass() {Id = "Root", Name = "Root"};
            }

            CurrentProperty.IsArray = true;
        }

        private void VisitEndArray(JsonTextReader reader)
        {
            if (!CurrentProperty.IsCustomType)
            {
                CurrentClass.AddProperties(CurrentProperty);
            }
            
            _propertyStack.Pop();
            _arrayState.Pop();
        }

        private void VisitStartObject(JsonTextReader reader)
        {
            if (CurrentProperty?.IsArray == true)
            {
                if (_arrayState.Peek().FirstObjectCreated)
                {
                    CurrentClass = new CommonClass();
                    CurrentClass.Id = _arrayState.Peek().ClassId;
                }
                else
                {
                    CurrentClass =new CommonClass();
                    CurrentClass.Id = GenerateClassId(CurrentProperty.Name);
                    CurrentClass.Name = CurrentClass.Id;
                    CurrentProperty.Type = CommonType.Custom;
                    CurrentProperty.IsCustomType = true;
                    _arrayState.Peek().ClassId = CurrentProperty.CustomTypeId = CurrentClass.Id;
                }

                return;
            }

            var createRootClass = CreateRootClass;
            CurrentClass = GetClassWithRootIfNecesarry();

            if (!createRootClass)
            {
                CurrentClass.Id = GenerateClassId(CurrentProperty.Name);
                CurrentClass.Name = CurrentClass.Id;
                CurrentProperty.Type = CommonType.Custom;
                CurrentProperty.IsCustomType = true;
                CurrentProperty.CustomTypeId = CurrentClass.Id;
            }
        }

        private string GenerateClassId(string name)
        {
            var id = _classStack.Skip(1).First()?.Id;
            return "\{id}_\{name}";
        }
        
        private CommonClass GetClassWithRootIfNecesarry()
        {
            if (!classes.Any() && CurrentClass == null)
            {
                return new CommonClass()
                {
                    Id = "Root",
                    Name = "Root"
                };
            }

            return new CommonClass();
        }

        private bool CreateRootClass => !classes.Any() && CurrentClass == null;

        private void VisitEndObject(JsonTextReader reader)
        {
            var candidate = _classStack.Pop();
            if (CurrentProperty?.IsArray == true && !_arrayState.Peek().FirstObjectCreated)
            {
                _arrayState.Peek().FirstObjectCreated = true;
            }

            if (candidate.Name == "Root")
            {
                _root = candidate;
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

            CurrentClass?.AddProperties(CurrentProperty);

            if (!CurrentProperty?.IsArray ?? false)
                _propertyStack.Pop();
            
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
