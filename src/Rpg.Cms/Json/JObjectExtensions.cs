using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Diagnostics.Contracts;

namespace Rpg.Cms.Json
{
    public static class JObjectExtensions
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Include
        };

        public static T? FromJson<T>(this string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        public static string? ToJson(this object? obj)
        {
            if (obj == null)
                return null;

            if (obj is JObject)
                return JsonConvert.SerializeObject((obj as JObject)!.ToCamelCase(), SerializerSettings);

            return JsonConvert.SerializeObject(obj, SerializerSettings);
        }

        public static JObject ToCamelCase(this JObject original)
        {
            var newObj = new JObject();
            foreach (var property in original.Properties())
            {
                var newPropertyName = property.Name.ToCamelCase();
                newObj[newPropertyName] = property.Value.ToCamelCaseJToken();
            }

            return newObj;
        }

        // Recursively converts a JToken with PascalCase names to camelCase
        [Pure]
        static JToken ToCamelCaseJToken(this JToken original)
        {
            switch (original.Type)
            {
                case JTokenType.Object:
                    return ((JObject)original).ToCamelCase();
                case JTokenType.Array:
                    return new JArray(((JArray)original).Select(x => x.ToCamelCaseJToken()));
                default:
                    return original.DeepClone();
            }
        }

        // Convert a string to camelCase
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        public static string ToPascalCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return char.ToUpperInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        public static IEnumerable<string> AllPropNames(this JObject node)
        {
            return node
                ?.Properties()
                ?.Select(x => x.Name)
                ?? Enumerable.Empty<string>();
        }

        public static bool AreEqual<T>(this JObject? jObj, string prop, T val)
        {
            var propExpr = new JPropExpression<T>(prop);
            var propVal = propExpr.Read(jObj, PropType.Source);

            return propVal.IsEqualTo(val);
        }

        public static bool AreEqual<T>(this JObject? target, JObject? source, string prop)
        {
            var propExpr = new JPropExpression<T>(prop);
            var targetVal = propExpr.Read(target, PropType.Target);
            var sourceVal = propExpr.Read(source, PropType.Source);

            return targetVal.IsEqualTo(sourceVal);
        }

        public static T[] Array<T>(this JObject? node, string prop)
        {
            var obj = node.Val<object>(prop);
            return obj.ToArray<T>();
        }

        public static T[] Array<T>(this JObject? node, string prop, string valueProp)
        {
            var objs = node.Array<JObject>(prop);

            return objs
                .Where(x => x.PropExists(valueProp))
                .Select(x => x.Val<object>(valueProp))
                .Where(x => x != null)
                .Cast<T>()
                .ToArray();
        }

        public static IEnumerable<JObject> MatchMany<T>(this JObject? node, string prop, string elementProp, T val)
        {
            var nodes = node.Val<JArray>(prop).ToList<JObject>();
            return nodes.Where(x => x.AreEqual(elementProp, val));
        }

        public static T? FirstVal<T>(this JObject? node, string prop, string elementProp, T elementVal, string matchedProp)
        {
            var nodes = node.Val<JArray>(prop).ToList<JObject>();
            var source = nodes.FirstOrDefault(x => x.AreEqual(elementProp, elementVal));

            return source.Val<T>(matchedProp);
        }

        public static T? Val<T>(this JObject? node, string prop)
        {
            var propExpr = new JPropExpression<T>(prop);
            var val = propExpr.Read(node, PropType.Source);

            if (typeof(T) == typeof(string))
                return (T)(val?.ToString() as object)!;

            if (val == null)
                return default;

            if (val is T)
                return (T)val;

            if (IsStringList<T>())
            {
                List<string> res = null;
                if (IsStringList(val))
                {
                    res = ((IEnumerable<string>)val).ToList();
                }
                else
                {
                    var strVal = val?.ToString() ?? string.Empty;
                    res = new List<string> { strVal };
                }
                if (typeof(T) == typeof(string[]))
                    return (T)(IEnumerable<string>)res.ToArray();

                return (T)(IEnumerable<string>)res;
            }

            return default;
        }

        public static bool PropIs<T>(this JObject node, string prop)
        {
            var val = node.Val<T>(prop);

            return val == null
                ? false
                : val is T;
        }

        public static bool PropIsCollectionOf<T>(this JObject node, string prop)
        {
            return node.Val<object>(prop)?.IsCollectionOf<T>() ?? false;
        }

        public static bool PropExists(this JObject? node, string prop)
        {
            var propExpr = new JPropExpression<object>(prop);
            return propExpr.Exists(node, PropType.Source);
        }

        public static JObject? AddNode(this JObject? node, string prop)
        {
            var propExpr = new JPropExpression<JObject>(prop);
            if (!propExpr.CanWrite(node, PropType.Source))
                return null;

            var newNode = new JObject();
            propExpr.Write(node, PropType.Source, newNode);

            return newNode;
        }

        public static JObject? Node(this JObject node, string prop)
            => new JPropExpression<JObject>(prop).Read(node, PropType.Source);

        public static IEnumerable<JObject> Nodes(this JObject node, string prop)
        {
            var propExpr = new JPropExpression<JArray>(prop);
            return (propExpr.Read(node, PropType.Source)?.Values<JObject>() ?? Enumerable.Empty<JObject>());
        }

        public static JObject AddProp<T>(this JObject node, string prop, T? value)
        {
            new JPropExpression<T>(prop)
                .Write(node, PropType.Source, value);

            return node;
        }

        public static JObject AddProp<T>(this JObject node, string prop, Func<JObject, T> propValFunc)
            => node.AddProp(prop, propValFunc.Invoke(node));

        public static JObject AddPropIfNotExists<T>(this JObject node, string prop, T? value)
        {
            var propExpr = new JPropExpression<T>(prop);
            if (!propExpr.Exists(node, PropType.Source))
                propExpr.Write(node, PropType.Source, value);

            return node;
        }

        public static JObject AddPropIfNotNull(this JObject node, string prop, object value)
        {
            return value == null
                ? node
                : AddProp(node, prop, value);
        }

        public static JObject RemoveProp(this JObject node, string prop)
        {
            new JPropExpression<object>(prop)
                .Remove(node, PropType.Source);

            return node;
        }

        public static JObject RemoveProps(this JObject node, params string[] parms)
        {
            if (parms != null)
                System.Array.ForEach(parms, prop => RemoveProp(node, prop));

            return node;
        }

        public static JObject KeepProps(this JObject node, params string[] parms)
        {
            if (node != null)
            {
                var removeProps = node.AllPropNames()
                    .Except(parms)
                    .ToArray();

                return node.RemoveProps(removeProps);
            }

            return node;
        }

        public static JObject CopyProp<T>(this JObject node, JObject? source, string sourceProp)
        {
            new JPropExpression<T>(sourceProp)
                .Copy(node, source);

            return node;
        }

        public static JObject CopyProp<T>(this JObject node, string targetProp, JObject? source, string sourceProp)
            => CopyProp<T>(node, source, $"{targetProp}:{sourceProp}");

        public static JObject CopyProps(this JObject? node, JObject? source, params string[] parms)
        {
            if (node == null || source == null)
                return node;

            parms = parms?.Any() == true
                ? parms
                : source.AllPropNames().ToArray();

            foreach (var parm in parms)
                node.CopyProp<object>(source, parm);

            return node;
        }

        public static JObject CopyPropsIfMissing(this JObject jObj, JObject? source, params string[] props)
        {
            if (props == null)
                return jObj;

            foreach (var prop in props)
            {
                var expr = new JPropExpression<object>(prop);
                if (!expr.Exists(jObj, PropType.Target))
                {
                    var val = expr.Read(source, PropType.Source);
                    expr.Write(jObj, PropType.Target, val);
                }
            }

            return jObj;
        }

        public static JObject CopyAllProps(this JObject? node, JObject? source)
            => CopyProps(node, source);

        public static JObject CopyAllExceptProps(this JObject? node, JObject? source, params string[] parms)
        {
            if (node == null || source == null)
                return node;

            if (parms == null || !parms.Any())
                return CopyAllProps(node, source);

            var props = source
                .AllPropNames()
                .Where(x => !parms.Contains(x))
                .ToArray();

            return CopyProps(node, source, props);
        }

        public static JObject Clone(this JObject? source, params string[] include) => source.Clone(include, null);

        public static JObject Clone(this JObject? source, string[]? include = null, string[]? exclude = null)
        {
            if (source == null)
                return null;

            var res = new JObject()
                .CopyProps(source, include);

            res = new JObject()
                .CopyAllExceptProps(res, exclude);

            return res;
        }

        public static JObject RenameProp(this JObject node, string prop)
        {
            var propExpr = new JPropExpression<object>(prop);
            propExpr.Rename(node, PropType.Source);

            return node;
        }

        public static JObject RenameProp(this JObject node, string prop, string newProp)
            => RenameProp(node, $"{prop}:{newProp}");

        private static bool IsStringList<T>()
        {
            return typeof(T) == typeof(IEnumerable<string>)
                || typeof(T) == typeof(List<string>)
                || typeof(T) == typeof(string[]);
        }

        private static bool IsStringList(object obj)
        {
            return obj.GetType() == typeof(IEnumerable<string>)
                || obj.GetType() == typeof(List<string>)
                || obj.GetType() == typeof(string[]);
        }

        public static bool IsEqualTo<TV>(this TV? val1, TV? val2)
        {
            if (typeof(TV).IsAssignableFrom(typeof(JToken)) && typeof(TV) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(TV)))
                throw new ArgumentException("AreEqual does not work on IEnumerable objects");

            string? s1;
            string? s2;
            if (typeof(TV).IsPrimitive || typeof(TV) == typeof(string))
            {
                s1 = val1?.ToString();
                s2 = val2?.ToString();
            }
            else if (typeof(TV).IsAssignableFrom(typeof(JToken)))
            {
                s1 = val1?.ToString();
                s2 = val2?.ToString();
            }
            else if (typeof(TV).IsClass)
            {
                s1 = val1 != null ? JObject.FromObject(val1)?.ToJson() : null;
                s2 = val2 != null ? JObject.FromObject(val2)?.ToJson() : null;
            }
            else
            {
                s1 = val1?.ToString();
                s2 = val2?.ToString();
            }

            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                return true;

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return false;

            return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static IEnumerable<T[]> Batches<T>(this IEnumerable<T> items, int batchSize)
        {
            if (items == null)
                return Enumerable.Empty<T[]>();

            var res = new List<T[]>();
            var batch = new List<T>();
            foreach (var item in items)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    res.Add(batch.ToArray());
                    batch.Clear();
                }
            }
            if (batch.Any())
                res.Add(batch.ToArray());

            return res;
        }

        public static JObject? ToJObject(this Dictionary<string, dynamic> attrs)
        {
            if (attrs == null)
                return null;

            var res = new JObject();

            var propNames = attrs.Keys;
            foreach (var prop in propNames)
            {
                object propVal = attrs[prop];
                if (propVal is JObject)
                {
                    propVal = (propVal as JObject)!;
                }
                else if (propVal.IsCollectionOf<JObject>())
                {
                    propVal = propVal.ToArray<JObject>();
                }
                else if (propVal.IsCollectionOf<object>())
                {
                    propVal = propVal.ToArray<object>();
                }

                if (propVal != null)
                    res.AddProp(prop, propVal);
            }

            return res;
        }
        public static JObject? ToJObject(this object? obj)
        {
            if (obj == null)
                return null;

            if (obj is JObject)
                return (JObject)obj;

            if (obj is JToken)
                return ((JToken)obj).ToJObject();

            if (obj is string)
                return JObject.Parse(obj as string);

            if (obj is JArray)
                throw new Exception("JArray Detected!");

            if (obj is Dictionary<string, dynamic>)
            {
                var json = obj.ToJson();

                return string.IsNullOrEmpty(json)
                    ? new JObject()
                    : JObject.Parse(json);
            }

            return JObject.FromObject(obj, new JsonSerializer { NullValueHandling = NullValueHandling.Include });
        }

        public static bool IsCollectionOf<T>(this object? obj)
        {
            if (obj == null)
                return false;

            if (obj is JArray)
                return ((JArray)obj).All(x => x.ToObject<object>() is T);

            if (obj is IEnumerable && !(obj is string) && !(obj is JToken))
                return ((IEnumerable)obj).ToEnumerable<object>().All(x => x is T);

            if (obj is string && typeof(T) == typeof(char))
                return true;

            return false;
        }

        public static IEnumerable<T> ToEnumerable<T>(this object? obj)
        {
            if (obj == null)
                return Enumerable.Empty<T>();

            if (obj is JArray)
                return ((JArray)obj)?.Select(x => x.ToObject<T>()!) ?? Enumerable.Empty<T>();


            if (obj is IEnumerable && !(obj is string) && !(obj is JToken))
                return EnumerableToList<T>((obj as IEnumerable)!);

            if (obj is string && typeof(T) == typeof(char))
                return obj.ToString()!.Cast<T>();

            var res = new List<T>();

            T? item = default(T?);
            if (obj is T)
            {
                item = (T)obj;
            }
            else if (typeof(T) == typeof(JObject))
            {
                item = (T?)(object?)obj.ToJObject();
            }
            else if (typeof(T) == typeof(string))
            {
                item = (T?)(object?)obj?.ToString()!;
            }

            if (item != null)
                res.Add(item);

            return res;
        }

        public static T[] ToArray<T>(this object? obj) => ToEnumerable<T>(obj).ToArray();
        public static List<T> ToList<T>(this object? obj) => ToEnumerable<T>(obj).ToList();

        private static List<T> EnumerableToList<T>(IEnumerable lst)
        {
            var res = new List<T>();
            foreach (var obj in lst)
            {
                if (obj != null)
                {
                    T? item;
                    if (obj is T)
                    {
                        item = (T?)obj;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        item = (T?)(object?)(obj?.ToString());
                    }
                    else if (typeof(T) == typeof(JObject))
                    {
                        item = (T?)(object?)obj.ToJObject();
                    }
                    else
                    {
                        item = (T?)obj;
                    }

                    if (item != null)
                        res.Add(item);
                }
            }

            return res;
        }
    }

}
