using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public interface IPropConverter
    {
        bool CanConvert(IPublishedProperty source);
        void Convert(JObject target, IPublishedProperty source);
    }
}
