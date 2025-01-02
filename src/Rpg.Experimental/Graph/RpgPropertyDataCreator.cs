using Rpg.Experimental.Activities;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using System.Reflection;

namespace Rpg.Experimental.Graph
{
    public class RpgPropertyDataCreator
    {
        public IRpgPropertyData[] CreatePropertyData(RpgObject obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(x => CreatePropertyData(obj.Id, x))
                .Where(x => x != null)
                .Cast<IRpgPropertyData>()
                .ToArray();
        }

        public IRpgPropertyData[] CreatePropertyData(RpgGraph graph, RpgAction action, RpgArg[] rpgArgs)
        {
            var res = new List<IRpgPropertyData>();
            foreach (var arg in rpgArgs)
            {
                if (graph.GetPropertyData(action.Id, arg.Name) == null)
                {
                    switch (arg.Type)
                    {
                        case nameof(Int32):
                            var intData = new RpgPropertyDataModdable(action.Id, arg.Name, RpgPropertyType.Int, arg.IsNullable);
                            res.Add(intData);
                            break;
                        case nameof(Dice):
                            var diceData = new RpgPropertyDataModdable(action.Id, arg.Name, RpgPropertyType.Dice, arg.IsNullable);
                            res.Add(diceData);
                            break;
                        default:
                            var refData = new RpgPropertyDataObject(action.Id, arg.Name, RpgPropertyType.Child);
                            res.Add(refData);
                            break;
                    }
                }
            }

            return res.ToArray();
        }

        private IRpgPropertyData? CreatePropertyData(string objectId, PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
                return null;

            if (propertyInfo.SetMethod == null)
                return null;

            return CreatePropertyData(objectId, propertyInfo.Name, propertyInfo.PropertyType);
        }

        public IRpgPropertyData? CreatePropertyData(string objectId, string prop, Type type)
        {
            if (RpgTypeUtilities.PropertyOfType(type, typeof(int)))
                return new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Int, RpgTypeUtilities.PropertyOfNullableType(type, typeof(int)));

            if (RpgTypeUtilities.PropertyOfType(type, typeof(Dice)))
                return new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Dice, RpgTypeUtilities.PropertyOfNullableType(type, typeof(Dice)));

            if (RpgTypeUtilities.PropertyOfType(type, typeof(RpgObject)))
                return new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Child);

            if (RpgTypeUtilities.PropertyOfType(type, typeof(ICollection<RpgObject>)))
                return new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Children);

            return null;
        }


        //private object? InitArgValue(RpgGraph graph, string argName)
        //{

        //    if (ActionArgs.Val(argName) != null)
        //        return ActionArgs.Val(argName);


        //    RpgObject? obj = argName switch
        //    {
        //        "owner" => graph.GetObject(ActionOwnerId),
        //        "initiator" => graph.GetObject(InitiatorId),
        //        "activity" => graph.GetLifespan(OwnerId) as Activity,
        //        "action" => this,
        //        _ => null
        //    };

        //    return obj;
        //}

        //private RpgPropertyRef? ArgNameToPropRef(RpgGraph graph, RpgAction action, string argName)
        //{
        //    var propParts = argName.Split('_');
        //    var propName = propParts[0];
        //    var obj = ArgNameToObject(graph, action, propName);

        //    if (obj is RpgObject rpgObj && propParts.Length > 1)
        //    {
        //        var prop = string.Join('.', propParts.Skip(1));
        //        return new RpgPropertyRef(rpgObj.Id, prop);
        //    }

        //    return null;
        //}

        //private object? ArgNameToObject(RpgGraph graph, RpgAction action, string argName)
        //{
        //    object? obj = argName switch
        //    {
        //        ReservedArgs.Owner => graph.GetObject(action.OwnerId),
        //        ReservedArgs.Initiator => graph.Actor,
        //        ReservedArgs.Action => action,
        //        ReservedArgs.Graph => graph,
        //        _ => null
        //    };

        //    return obj;
        //}
    }
}
