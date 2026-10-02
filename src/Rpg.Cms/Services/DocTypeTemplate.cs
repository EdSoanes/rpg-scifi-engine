using Rpg.Experimental.System;

namespace Rpg.Cms.Services
{
    /// <summary>
    /// The names of the data types every game system gets. One per kind of value an author can enter.
    /// </summary>
    public static class RpgDataTypes
    {
        public const string Integer = "Integer";
        public const string Dice = "Dice";
        public const string Text = "Text";
        public const string LongText = "LongText";
        public const string Boolean = "Boolean";

        /// <summary>A picker for the things a parent holds, e.g. the items in a character's hands</summary>
        public const string Children = "Children";

        public static string For(EditorType editor)
            => editor switch
            {
                EditorType.Int32 => Integer,
                EditorType.Select => Integer,
                EditorType.Dice => Dice,
                EditorType.Boolean => Boolean,
                EditorType.RichText => LongText,
                EditorType.LongText => LongText,
                EditorType.Child => Children,
                EditorType.Children => Children,
                _ => Text
            };
    }

    /// <summary>
    /// A property of a document type
    /// </summary>
    public class DocTypeProp
    {
        /// <summary>The name of the property on the game system object, e.g. HitBonus</summary>
        public string Prop { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>One of the names in RpgDataTypes</summary>
        public string DataTypeName { get; set; } = RpgDataTypes.Text;
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// The alias of the property in the content management system: the property name made safe, with a
        /// lower case first letter, e.g. hitBonus
        /// </summary>
        public string Alias
        {
            get
            {
                var safe = new string(Prop.Where(char.IsLetterOrDigit).ToArray());
                return safe.Length > 0
                    ? char.ToLowerInvariant(safe[0]) + safe.Substring(1)
                    : safe;
            }
        }

        public override string ToString()
            => $"{Prop} ({DataTypeName})";
    }

    /// <summary>
    /// What a document type should look like. Built from a game system object, or by hand for the
    /// document types that hold a system's libraries together.
    /// </summary>
    public class DocTypeTemplate
    {
        public string Archetype { get; }
        public string? Icon { get; private set; }
        public bool AllowedAsRoot { get; private set; }
        public bool IsElement { get; private set; }
        public List<DocTypeProp> Props { get; } = new();

        /// <summary>The aliases of the document types allowed beneath this one</summary>
        public List<string> AllowedChildAliases { get; } = new();

        public DocTypeTemplate(string archetype)
            => Archetype = archetype;

        public DocTypeTemplate AddIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        public DocTypeTemplate SetIsElement(bool isElement)
        {
            IsElement = isElement;
            return this;
        }

        public DocTypeTemplate AllowAsRoot(bool allow)
        {
            AllowedAsRoot = allow;
            return this;
        }

        public DocTypeTemplate AddProp(string prop, string dataTypeName, string? displayName = null, string? tab = null, string? group = null)
        {
            if (!Props.Any(x => x.Prop == prop))
                Props.Add(new DocTypeProp
                {
                    Prop = prop,
                    DisplayName = displayName ?? prop,
                    DataTypeName = dataTypeName,
                    Tab = tab ?? string.Empty,
                    Group = group ?? string.Empty
                });

            return this;
        }

        public DocTypeTemplate AddAllowedChild(string alias)
        {
            if (!AllowedChildAliases.Contains(alias))
                AllowedChildAliases.Add(alias);

            return this;
        }

        public override string ToString()
            => $"{Archetype} ({Props.Count} properties)";
    }
}
