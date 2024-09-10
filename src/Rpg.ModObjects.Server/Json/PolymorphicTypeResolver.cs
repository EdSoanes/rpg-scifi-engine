using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Server.Json
{
    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            Type baseRpgObjectType = typeof(RpgObject);
            if (jsonTypeInfo.Type == baseRpgObjectType)
            {
                foreach (var assembly in RpgTypeScan.GetScanAssemblies())
                {
                    RpgTypeScan.ForTypes<RpgObject>()
                }

                    jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$obj-type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                {
                    new JsonDerivedType(typeof(ThreeDimensionalPoint), "3d"),
                    new JsonDerivedType(typeof(FourDimensionalPoint), "4d")
                }
                };
            }

            return jsonTypeInfo;
        }
    }
}
