using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public interface IRpgArgFactory
    {
        bool CanCreate(ParameterInfo parameterInfo);
        RpgArg Create(ParameterInfo parameterInfo);
    }
}