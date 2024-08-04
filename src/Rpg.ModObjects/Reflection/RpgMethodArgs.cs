using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public static class RpgMethodArgs
    {
        public static Type[] PrimitiveArgTypes => [typeof(int), typeof(Dice), typeof(string)];

        private static IRpgArgFactory[]? _argFactories;
        private static IRpgArgFactory[] GetArgFactories()
        {
            if (_argFactories == null)
            {
                var nspc = typeof(RpgObject).Namespace!;
                var types = RpgTypeScan.ForTypes<IRpgArgFactory>()
                    .Where(x => !x.IsAbstract && !string.IsNullOrEmpty(x.Namespace) && !x.Namespace.StartsWith(nspc));

                var factories = types
                    .Select(x => Activator.CreateInstance(x) as IRpgArgFactory)
                    .Where(x => x != null)
                    .Cast<IRpgArgFactory>()
                    .ToList();

                factories.Add(Activator.CreateInstance<InitiatorFactory>());
                factories.Add(Activator.CreateInstance<OwnerFactory>());
                factories.Add(Activator.CreateInstance<ActivityFactory>());
                factories.Add(Activator.CreateInstance<DefaultObjectFactory>());
                factories.Add(Activator.CreateInstance<IntegerArgFactory>());
                factories.Add(Activator.CreateInstance<DiceArgFactory>());
                factories.Add(Activator.CreateInstance<DefaultArgFactory>());

                _argFactories = factories.ToArray();
            }

            return _argFactories;
        }

        public static RpgArg[] CreateArgs(MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters()
                .Select(CreateArg)
                .Where(x => x != null)
                .Cast<RpgArg>()
                .ToArray();

            return args;
        }

        private static RpgArg? CreateArg(ParameterInfo parameterInfo)
        {
            var factory = GetArgFactories().FirstOrDefault(x => x.CanCreate(parameterInfo));
            return factory?.Create(parameterInfo);
        }

    }
}
