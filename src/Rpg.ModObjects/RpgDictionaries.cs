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
    }

    public class StatesDictionary : Dictionary<string, State>
    {
        public StatesDictionary() : base() { }
    }

    public class ActionsDictionary : Dictionary<string, Actions.Action>
    {
        public ActionsDictionary() : base() { }
    }
}
