using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public interface IRpgPropertyData : ILifecycle
    {
        string ObjectId { get; }
        string Prop { get; }
        RpgPropertyType PropType { get; }
        bool IsNullable { get; }
        bool IsVirtual { get; }

        RpgProperty GetProperty(RpgGraph graph);
        T? GetValue<T>(RpgGraph graph);
        void OnCreatingVirtual(RpgGraph graph, object? value);
        void OnSyncProperty(RpgGraph graph, string prop);
    }
}
