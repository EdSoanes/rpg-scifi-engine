using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Actions
{
    internal class ActionMethod<TReturn> : RpgMethod<Action, TReturn>
    {
        [JsonConstructor] private ActionMethod() { }
        
        public ActionMethod(Action owner, string methodName)
            : base(owner, methodName)
        { }
    }
}
