namespace Rpg.Cms.Models
{
    public class Act
    {
        public string OwnerId { get; set; }
        public string InitiatorId { get; set; }
        public string ActionName { get; set; }
        public int ActionNo { get; set; }

        public Dictionary<string, string?> ArgValues { get; set; } = new();
    }
}
