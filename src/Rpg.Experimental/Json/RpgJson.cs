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

                    //Saved sheets are for the engine to read, not people: no indentation, and nothing that a
                    //restore would get right without being told
                    Formatting = Formatting.None,
                    ContractResolver = new RpgGraphStateContractResolver(false, true)
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
        /// A graph state as compressed text. Saved sheets are very repetitive, so this is many times
        /// smaller than SerializeGraphState().
        /// </summary>
        public static string SerializeSnapshot(object obj)
        {
            var json = SerializeGraphState(obj);
            var bytes = global::System.Text.Encoding.UTF8.GetBytes(json);

            using var output = new MemoryStream();
            using (var gzip = new global::System.IO.Compression.GZipStream(output, global::System.IO.Compression.CompressionLevel.Optimal))
                gzip.Write(bytes, 0, bytes.Length);

            return Convert.ToBase64String(output.ToArray());
        }

        /// <summary>
        /// Restore a graph state from compressed text. Text that is not compressed is accepted too.
        /// </summary>
        public static T DeserializeSnapshot<T>(string snapshot)
            where T : class
        {
            if (snapshot.TrimStart().StartsWith('{'))
                return DeserializeGraphState<T>(snapshot);

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
