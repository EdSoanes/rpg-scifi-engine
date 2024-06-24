using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cms.Json
{
    public class JPropExpression<T>
    {
        private string _expr;

        public JProp<T> Source { get; private set; }
        public JProp<T> Target { get; private set; }

        private bool SamePropExpressions = false;

        private T? DefaultValue;

        public JPropExpression(string propExpr)
        {
            _expr = propExpr;

            var propstrs = Parse(propExpr);

            if (propstrs.Length is < 1 or > 2)
                throw new JOpException($"Invalid property expression {propExpr}");

            if (propstrs.Length == 1)
            {
                Target = new JProp<T>(propstrs[0]);
                Source = new JProp<T>(propstrs[0]);

                SamePropExpressions = true;
            }
            else if (propstrs.Length == 2)
            {
                Target = new JProp<T>(propstrs[0]);
                Source = new JProp<T>(propstrs[1]);
            }
        }

        public JPropExpression(string propExpr, T? defaultValue)
            : this(propExpr)
        {
            DefaultValue = defaultValue;
        }

        protected JProp<T> GetProp(PropType propType) => propType switch
        {
            PropType.Source => Source,
            _ => Target
        };

        protected JProp<T> GetOtherProp(PropType propType) => propType switch
        {
            PropType.Source => Target,
            _ => Source
        };

        internal bool CanRead(JToken? jObj, PropType propType) => jObj != null && !string.IsNullOrEmpty(GetProp(propType).Name);

        internal bool CanWrite(JToken? jObj, PropType propType) => jObj != null && !string.IsNullOrEmpty(GetProp(propType).Name);

        internal bool CanCopy(JToken? target, JToken? source) => CanRead(source, PropType.Source) && CanWrite(target, PropType.Target);

        internal bool CanRename(JObject? jObj, PropType propType)
            => CanWrite(jObj, propType)
                && !string.IsNullOrEmpty(Source.Name)
                && !Source.Name.Equals(Target.Name)
                && !Source.Path.Any();

        public bool Exists(JObject? jObj, PropType propType)
        {
            if (CanRead(jObj, propType))
            {
                var prop = GetProp(propType);
                return GetNodeFromPath(prop.Path, jObj)?.Properties().Any(x => x.Name == prop.Name) ?? false;
            }

            return false;
        }

        public T? Read(JObject? jObj, PropType propType)
        {
            if (CanRead(jObj, propType))
            {
                var prop = GetProp(propType);
                jObj = GetNodeFromPath(prop.Path, jObj);
                return Val(jObj, prop.Name) ?? DefaultValue;
            }

            return DefaultValue;
        }

        public T? Val(JObject? obj, string prop)
        {
            if (obj == null)
                return default;

            try
            {
                if (obj.PropExists(prop))
                {
                    var rawVal = obj[prop];
                    T? val = rawVal != null ? rawVal.ToObject<T>() : default;
                    return val ?? default;
                }

                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public JPropAction? Copy(JObject? target, JObject? source)
        {
            if (!CanCopy(target, source))
                return null;

            var val = Read(source, PropType.Source);
            return Write(target, PropType.Target, val);
        }

        public JPropAction? Write(JObject? jObj, PropType propType, T? val)
        {
            if (!CanWrite(jObj, propType))
                return null;

            var prop = GetProp(propType);
            val = val ?? DefaultValue;
            var oldVal = GetNodeFromPath(prop.Path, jObj).Val<object>(prop.Name);

            if (!(val?.ToString()).IsEqualTo(oldVal?.ToString()) || !jObj.PropExists(prop.Name))
            {
                var updated = false;
                jObj = AddNodeFromPath(prop.Path, jObj!)!;
                if (jObj.PropExists(prop.Name))
                {
                    jObj.Remove(prop.Name);
                    updated = true;
                }

                jObj.Add(new JProperty(Target.Name, val));
                return new JPropAction
                {
                    Op = updated ? Ops.Update : Ops.Insert,
                    TargetProp = Target.Expr,
                    SourceProp = Source.Expr,
                    OldVal = oldVal,
                    NewVal = val
                };
            }

            return null;
        }

        internal string? Remove(JObject? jObj, PropType propType)
        {
            if (CanWrite(jObj, propType))
            {
                var prop = GetProp(propType);
                var removed = GetNodeFromPath(prop.Path, jObj)?.Remove(prop.Name) ?? false;
                return $"{(removed ? "-" : "")}{prop.Expr}";
            }

            return null;
        }

        internal void Rename(JObject? jObj, PropType propType)
        {
            if (CanRename(jObj, propType))
            {
                var targetProp = GetProp(propType);
                var sourceProp = GetOtherProp(propType);
                jObj = GetNodeFromPath(targetProp.Path, jObj);
                if (jObj != null)
                {
                    var val = jObj[targetProp.Name];
                    jObj.Remove(targetProp.Name);
                    jObj[sourceProp.Name] = val;
                }
            }
        }

        private string[] Parse(string propExpr)
            => propExpr?
                .TrimStart('{')
                .TrimEnd('}')
                .Split(':')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray() ?? new string[0];


        private JObject? GetNodeFromPath(string[] path, JObject? source)
        {
            foreach (var seg in path)
            {
                source = source?[seg] as JObject;
                if (source == null)
                    break;
            }

            return source;
        }

        private JObject? AddNodeFromPath(string[] path, JObject target)
        {
            if (target == null)
                return null;

            var currNode = target;
            foreach (var seg in path)
            {
                var nextNode = currNode?[seg] as JObject;
                if (nextNode == null)
                {
                    nextNode = new JObject();
                    currNode[seg] = nextNode;
                }

                currNode = nextNode;
            }
            return currNode;
        }
    }
}
