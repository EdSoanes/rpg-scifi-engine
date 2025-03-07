using Newtonsoft.Json;

namespace Rpg.Experimental.Json
{
    public class RpgJson
    {
        public static JsonSerializerSettings? _graphStateOptions;
        public static JsonSerializerSettings? _modelOptions;

        public static JsonSerializerSettings GraphStateOptions()
        {
            if (_graphStateOptions == null)
            {
                _graphStateOptions = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.Indented,
                    ContractResolver = new RpgGraphStateContractResolver(false)
                };

                _graphStateOptions.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }

            return _graphStateOptions;
        }

        public static JsonSerializerSettings ModelOptions()
        {
            if (_modelOptions == null)
            {
                _modelOptions = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.Indented,
                    ContractResolver = new RpgGraphStateContractResolver(true)
                };

                _modelOptions.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }

            return _modelOptions;
        }

        public static string SerializeGraphState(object obj)
            => JsonConvert.SerializeObject(obj, GraphStateOptions());

        public static T DeserializeGraphState<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, GraphStateOptions())!;

        public static string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, ModelOptions());

        public static T Deserialize<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, ModelOptions())!;

        public static T Deserialize<T>(Type type, string json)
            where T : class
                => (T)JsonConvert.DeserializeObject(json, type, GraphStateOptions())!;
    }
}
