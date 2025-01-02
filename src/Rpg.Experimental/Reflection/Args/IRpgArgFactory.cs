using System.Reflection;

namespace Rpg.Experimental.Reflection.Args
{
    public interface IRpgArgFactory
    {
        bool CanCreate(ParameterInfo parameterInfo);
        RpgArg Create(ParameterInfo parameterInfo);
    }
}