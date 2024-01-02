using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaEntity
    {
        public Guid? Id { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public MetaAction[] AbilityMethods { get; set; } = new MetaAction[0];

        public override string ToString()
        {
            return $"{Path} => {Type}({Id})";
        }
    }
}
