
namespace Rpg.ModObjects
{
    public interface IModState : ITemporal
    {
        Guid EntityId { get; }
        string InstanceName { get; }
        string Name { get; }

        void SetActive();
        void SetInactive();
    }
}