using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sigill.Common.ActionResults
{
    public static class Extensions
    {
        private static Lazy<JsonSerializerSettings> SerializerSettings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new StringEnumConverter());

            return settings;
        });

        public static string ToJson<T>(this T? obj)
            where T : class
                => JsonConvert.SerializeObject(obj, SerializerSettings.Value);

        public static T? FromJson<T>(this string? json)
            where T : class
                => string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);

        public static IActionResult ToJsonResult<T>(this T? obj)
            where T : class
        {
            if (obj != null)
            {
                return new ContentResult
                {
                    Content = obj.ToJson(),
                    ContentType = "application/json"
                };
            }
            else
            {
                return new NoContentResult();
            }
        }
    }
}
