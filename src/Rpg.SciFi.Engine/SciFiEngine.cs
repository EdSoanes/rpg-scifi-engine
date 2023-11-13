using Newtonsoft.Json;
using Rpg.SciFi.Engine.Turns;
using System.Reflection;

namespace Rpg.SciFi.Engine
{
    public class UseAbilityInfo
    {
        public string Name { get; set; }
        public List<UseAbilityParameterInfo> Parameters { get; set; } = new List<UseAbilityParameterInfo>();
    }

    public class UseAbilityParameterInfo
    {
        public string Name { set; get; }
        public string Type { set; get; }
        public string Input { set; get; }
    }


    public static class SciFiEngine
    {
        public static CharacterSheet CharacterSheet { get; set; } = new CharacterSheet();
        public static Encounter Encounter { get; set; } = new Encounter();

        public static class UserActions
        {
            public static Action<int>? RollDamage(Artifact artifact)
            {
                return (roll) =>
                {
                    //Get damage roll from user and present results
                };
            }
        }

        public static string Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return json;
        }

        public static string? Evaluate(string binding)
        {
            return GetValue(binding)?.ToString();
        }

        //public static TurnAction Use(Artifact artifact, Ability ability)
        //{
        //    TurnActionResult? res = null;

        //    MethodInfo? method = ability.GetType().GetMethod("Use");
        //    if (method != null)
        //    {
        //        var args = method.GetParameters();
        //        if (args.Any())
        //        {
        //            var argVals = new List<object?>();
        //            var attrs = method.GetCustomAttributes<InputAttribute>();
        //            foreach (var arg in args)
        //            {
        //                var attr = attrs.FirstOrDefault(x => x.Param == arg.Name);
        //                if (attr != null)
        //                {
        //                    if (attr.BindsTo == "this")
        //                        argVals.Add(artifact);
        //                    else
        //                        argVals.Add(GetValue(attr.BindsTo));
        //                }
        //            }

        //            res = (TurnActionResult?)method.Invoke(ability, argVals.ToArray());
        //        }
        //        else
        //            res = (TurnActionResult?)method.Invoke(ability, null);

        //    }

        //    return res;
        //}

        public static UseAbilityInfo[] EnumerateAbilities()
        {
            var abilities = EnumerateAbilities(null, Assembly.GetExecutingAssembly().GetTypes());
            return abilities;
        }

        private static UseAbilityInfo[] EnumerateAbilities(string? nameSuffix, Type[] types)
        {
            var abilities = new List<UseAbilityInfo>();

            foreach (Type type in types)
            {
                var useAbilityInfo = GetUseAbilityInfo(nameSuffix, type);
                if (useAbilityInfo != null)
                    abilities.Add(useAbilityInfo);

                var nestedTypes = type.GetNestedTypes();
                if (nestedTypes.Any())
                {
                    var suffix = string.IsNullOrEmpty(nameSuffix)
                        ? type.Name
                        : $"{nameSuffix}.{type.Name}";

                    var nestedUseAbilityInfos = EnumerateAbilities(suffix, nestedTypes);
                    if(nestedUseAbilityInfos.Any())
                        abilities.AddRange(nestedUseAbilityInfos);
                }
            }

            return abilities.ToArray();
        }

        private static UseAbilityInfo? GetUseAbilityInfo(string? nameSuffix, Type type)
        {
            MethodInfo? method = type.GetMethod("Use");
            if (method != null && method.ReturnType == typeof(TurnAction[]))
            {
                var useAbilityInfo = new UseAbilityInfo
                {
                    Name = !string.IsNullOrEmpty(nameSuffix)
                        ? $"{type.Name}.{nameSuffix}"
                        : type.Name
                };

                var attrs = method.GetCustomAttributes<InputAttribute>();
                foreach (var arg in method.GetParameters())
                {
                    var attr = attrs.FirstOrDefault(x => x.Param == arg.Name);
                    useAbilityInfo.Parameters.Add(new UseAbilityParameterInfo
                    {
                        Name = arg.Name!,
                        Type = arg.ParameterType.Name,
                        Input = attr?.BindsTo ?? "UNKNOWN"
                    });
                }

                return useAbilityInfo;
            }

            return null;
        }

        private static object? GetValue(string binding)
        {
            var parts = binding.Split('.');

            var propertyInfo = typeof(SciFiEngine).GetProperty(parts[0]);
            if (propertyInfo == null)
                throw new Exception("Invalid path start");

            var propVal = propertyInfo.GetValue(null, null);
            if (propVal == null)
            {
                if (parts.Length == 1)
                    return null;
                else
                    throw new Exception("Invalid path start");
            }

            return GetValue(propVal, parts.Skip(1).ToArray());
        }

        private static object? GetValue(object? obj, string[] path)
        {
            if (path.Length == 0)
                throw new Exception("Empty Path");

            var propInfo = obj?.GetType().GetProperty(path[0]);
            if (propInfo == null)
                throw new Exception("Invalid Path");

            var propVal = propInfo.GetValue(obj);
            if (path.Length > 1)
            {
                if (propVal == null)
                    throw new Exception("Invalid Path, null value");

                return GetValue(propVal, path.Skip(1).ToArray());
            }

            return propVal;
        }
    }
}
