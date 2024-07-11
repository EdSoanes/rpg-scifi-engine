using Newtonsoft.Json.Linq;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public interface IPropConverter
    {
        bool CanConvert(IPublishedProperty source);
        void Convert(IMetaSystem system, ContentConverter contentConverter, JObject target, IPublishedProperty source, string fullPropName);
    }
}
