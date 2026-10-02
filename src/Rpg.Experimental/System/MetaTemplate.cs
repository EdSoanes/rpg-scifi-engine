using Newtonsoft.Json;

namespace Rpg.Experimental.System
{
    /// <summary>
    /// What has to be supplied to create an object of a type: the values an author fills in.
    ///
    /// A game system object declares it by having a public constructor that takes a single template class,
    /// e.g. new MeleeWeapon(MeleeWeaponTemplate template). The settable properties of the template class
    /// are the values to author. Everything else about the object is worked out by the rules.
    /// </summary>
    public class MetaTemplate
    {
        /// <summary>The name of the template class, e.g. MeleeWeaponTemplate</summary>
        [JsonProperty] public string TypeName { get; init; } = string.Empty;

        [JsonProperty] public string QualifiedTypeName { get; init; } = string.Empty;

        /// <summary>
        /// The values to author. Where the object has a property of the same name, its display name, tab,
        /// group and other attributes are used.
        /// </summary>
        [JsonProperty] public List<MetaProperty> Properties { get; init; } = new();

        public override string ToString()
            => $"{TypeName} ({Properties.Count} properties)";
    }
}
