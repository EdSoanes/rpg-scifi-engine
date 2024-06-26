﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public class RpgSerializer
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        public static string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, JsonSettings);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, JsonSettings)!;

        public static object? Deserialize(Type type, string json)
            => JsonConvert.DeserializeObject(json, type, JsonSettings)!;

    }
}
