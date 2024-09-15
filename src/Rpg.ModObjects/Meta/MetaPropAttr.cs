namespace Rpg.ModObjects.Meta
{
    public class MetaPropAttr : Dictionary<string, object?>
    {
        public EditorType? Editor { get => ContainsKey("Editor") && this["Editor"] is EditorType ? (EditorType)this["Editor"]! : null; }

        public T Value<T>(string prop, T def)
        {
            if (TryGetValue(prop, out var val) && val != null)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)(val.ToString() ?? string.Empty);

                if (val is T)
                    return (T)val;
            }

            return def;
        }
    }
}
