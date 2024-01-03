using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Core;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class MetaEntity
    {
        public Guid? Id { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public MetaAbility[] AbilityMethods { get; set; } = new MetaAbility[0];

        public override string ToString()
        {
            return $"{Path} => {Type}({Id})";
        }

        public static MetaEntity Create(ModdableObject entity)
        {
            var metaEntity = new MetaEntity
            {
                Id = entity.Id,
                Type = entity.GetType().Name,
                AbilityMethods = GetAbilityMethods(entity)
            };

            return metaEntity;
        }

        private static MetaAbility[] GetAbilityMethods(ModdableObject entity)
        {
            var metaAbilityMethods = new List<MetaAbility>();

            foreach (var methodInfo in entity.GetType().GetMethods())
            {
                var attr = methodInfo.GetCustomAttribute<AbilityAttribute>();
                if (attr == null || methodInfo.ReturnType != typeof(TurnAction))
                    continue;

                var metaAbilityMethod = new MetaAbility
                {
                    Name = methodInfo.Name
                };

                var inputAttrs = methodInfo.GetCustomAttributes<InputAttribute>();
                foreach (var parameter in methodInfo.GetParameters())
                {
                    var inputAttr = inputAttrs.FirstOrDefault(x => x.Param == parameter.Name);
                    if (inputAttr == null)
                        throw new ArgumentException($"{methodInfo.Name} missing matching Input attribute");

                    var metaActionInput = new MetaAbilityInput
                    {
                        Name = inputAttr.Param,
                        BindsTo = inputAttr.BindsTo,
                        InputSource = inputAttr.InputSource
                    };

                    metaAbilityMethod.Inputs.Add(metaActionInput);
                }

                metaAbilityMethods.Add(metaAbilityMethod);
            }

            return metaAbilityMethods.ToArray();
        }
    }
}
