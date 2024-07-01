namespace Rpg.ModObjects.Actions
{
    internal class ActionStore : RpgBaseStore<string, Action>
    {
        public ActionStore(string entityId)
            : base(entityId) { }

        public void Add(params Action[] actions)
        {
            foreach (var action in actions)
            {
                if (!Contains(action))
                {
                    action.OnBeforeTime(Graph!);
                    Items.Add(action.Name, action);
                }
            }
        }
    }
}
