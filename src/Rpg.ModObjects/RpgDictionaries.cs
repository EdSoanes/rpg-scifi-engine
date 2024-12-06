using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects
{
    public class ObjectsDictionary : Dictionary<string, RpgObject>
    {
        public ObjectsDictionary() : base() { }
    }

    public class ModSetDictionary : Dictionary<string, ModSet>
    {
        public ModSetDictionary() : base() { }
    }

    public class PropsDictionary : Dictionary<string, Prop>
    {
        public PropsDictionary() : base() { }

        internal Prop? GetProp(RpgGraph graph, RpgObject rpgObj, string prop, RefType refType = RefType.Value, bool create = false)
        {
            if (string.IsNullOrEmpty(prop))
                return null;

            if (ContainsKey(prop))
                return this[prop];

            if (create)
            {
                var created = new Prop(rpgObj.Id, prop, refType);
                if (graph != null)
                {
                    created.OnCreating(graph, rpgObj);
                    created.OnTimeBegins();
                    created.OnStartLifecycle();
                }

                Add(prop, created);

                return created;
            }

            return null;
        }

        internal Mod? GetMod(string id)
            => Values
                .SelectMany(x => x.Mods)
                .FirstOrDefault(x => x.Id == id);

        public Mod[] GetMods()
            => Values
                .SelectMany(x => x.Mods)
                .ToArray();

        public Mod[] GetMods(string prop)
            => ContainsKey(prop)
            ? this[prop].Mods.ToArray()
            : [];
    }

    public class StatesDictionary : Dictionary<string, State>
    {
        public StatesDictionary() : base() { }
    }

    public class ActionTemplateDictionary : Dictionary<string, ActionTemplate>
    {
        public ActionTemplateDictionary() : base() { }
    }
}
