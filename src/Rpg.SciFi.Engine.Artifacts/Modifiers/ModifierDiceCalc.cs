using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModifierDiceCalc
    {
        [JsonProperty] public Guid? EntityId { get; private set; }
        [JsonProperty] public string? ClassName { get; private set; }
        [JsonProperty] public string? FuncName { get; private set; }

        public bool IsStatic => !string.IsNullOrWhiteSpace(ClassName)
            && !string.IsNullOrWhiteSpace(FuncName);

        public bool IsCalc => !string.IsNullOrWhiteSpace(FuncName)
            && (EntityId != null || !string.IsNullOrWhiteSpace(ClassName));

        [JsonConstructor] private ModifierDiceCalc() { }

        public ModifierDiceCalc(string? diceCalc) 
        {
            if (!string.IsNullOrEmpty(diceCalc))
            {
                var parts = diceCalc.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    if (Guid.TryParse(parts[0], out var id))
                        EntityId = id;
                    else
                        ClassName = parts[0];
                }

                FuncName = parts.Last();
            }
        }

        public void SetDiceCalc<T>(Expression<Func<Func<T, T>>>? expression)
        {
            if (expression == null)
                return;

            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MethodInfo)methodInfoExpression.Value!;

            if (methodInfo.IsStatic)
            {
                ClassName = methodInfo.DeclaringType!.Name;
                FuncName = methodInfo.Name;
                return;
            }

            Entity? entity = null;
            var memberExpression = methodCallExpression.Arguments?.Last() as MemberExpression;
            if (memberExpression != null)
            {
                var constantExpression = memberExpression.Expression as ConstantExpression;
                var fieldInfo = memberExpression?.Member as FieldInfo;
                entity = fieldInfo?.GetValue(constantExpression?.Value) as Entity;
            }
            else
            {
                var constantExpression = methodCallExpression.Arguments?.Last() as ConstantExpression;
                entity = constantExpression?.Value as Entity;
            }

            EntityId = entity?.Id;
            FuncName = methodInfo.Name;
            return;
        }

        public Dice Apply(Dice dice, Entity? entity)
        {
            if (!IsCalc)
                return dice;

            if (IsStatic)
                return this.ExecuteFunction<Dice, Dice>($"{ClassName}.{FuncName}", dice);

            if (entity != null && entity.Id == EntityId)
                return entity.ExecuteFunction<Dice, Dice>(FuncName!, dice);

            return dice;
        }
    }
}
