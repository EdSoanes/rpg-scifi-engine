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

        /// <summary>
        /// A graph state as compact, compressed text
        /// </summary>
        public static string SerializeSnapshot(object obj)
        {
            var options = GraphStateOptions();
            var compact = new JsonSerializerSettings
            {
                ReferenceLoopHandling = options.ReferenceLoopHandling,
                TypeNameHandling = options.TypeNameHandling,
                NullValueHandling = options.NullValueHandling,
                Formatting = Formatting.None,
                ContractResolver = options.ContractResolver,
                Converters = options.Converters
            };

            var json = JsonConvert.SerializeObject(obj, compact);
            var bytes = global::System.Text.Encoding.UTF8.GetBytes(json);

            using var output = new MemoryStream();
            using (var gzip = new global::System.IO.Compression.GZipStream(output, global::System.IO.Compression.CompressionLevel.Optimal))
                gzip.Write(bytes, 0, bytes.Length);

            return Convert.ToBase64String(output.ToArray());
        }

        public static T DeserializeSnapshot<T>(string snapshot)
            where T : class
        {
            using var input = new MemoryStream(Convert.FromBase64String(snapshot));
            using var gzip = new global::System.IO.Compression.GZipStream(input, global::System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, global::System.Text.Encoding.UTF8);

            return DeserializeGraphState<T>(reader.ReadToEnd());
        }

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
