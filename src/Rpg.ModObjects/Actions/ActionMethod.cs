using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    internal class ActionMethod<TOwner, TReturn>
        where TOwner : RpgObject
    {
        [JsonProperty] public string MethodName { get; set; }
        [JsonProperty] public RpgActionArg[] Args { get; private set; } = new RpgActionArg[0];

        public ActionMethod(ActionModification<TOwner> owner, string methodName) 
        {
            MethodName = methodName;
            EnsureActionMethod(owner);
        }

        public ActionMethodArgs GetActionMethodArgs()
            => new ActionMethodArgs(Args);

        public TReturn Execute(TOwner owner, ActionMethodArgs actionMethodArgs)
        {
            var modifications = owner.ExecuteFunction<TReturn>(MethodName, actionMethodArgs.ToArgs())!;
            return modifications;
        }

        private void EnsureActionMethod(ActionModification<TOwner> owner)
        {
            var methodInfo = owner.GetType().GetMethod(MethodName);
            if (methodInfo == null)
                throw new InvalidOperationException($"{MethodName}() method not found on ActionModification class");

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(TReturn)))
                throw new InvalidOperationException($"{MethodName}() method does not have return type {nameof(TReturn)}");

            Args = methodInfo.GetParameters()
                .Select(x => new RpgActionArg(x, null))
                .ToArray();
        }
    }
}
