namespace Rpg.Sys.Actions
{
    public struct ActionCost
    {
        public int Action { get; set; }
        public int Exertion { get; set; }
        public int Focus { get; set; }

        public ActionCost() { }
        public ActionCost(int action, int exertion, int focus)
        {
            Action = action;
            Exertion = exertion;
            Focus = focus;
        }

        public static ActionCost operator +(ActionCost d1, ActionCost d2)
        {
            return new ActionCost
            {
                Action = d1.Action + d2.Action,
                Exertion = d1.Exertion + d2.Exertion,
                Focus = d1.Focus + d2.Focus
            };
        }
    }
}
