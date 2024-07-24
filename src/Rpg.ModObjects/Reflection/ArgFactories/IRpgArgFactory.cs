using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public interface IRpgArgFactory
    {
        bool CanCreate(ParameterInfo parameterInfo);
        RpgArg Create(ParameterInfo parameterInfo);
    }
}