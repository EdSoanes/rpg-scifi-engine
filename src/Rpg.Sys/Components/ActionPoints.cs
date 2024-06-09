using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rpg.Sys.Components
{
    public class ActionPoints : RpgComponent
    {
        [JsonProperty] public MinMaxValue Action { get; private set; }
        [JsonProperty] public MinMaxValue Exertion { get; private set; }
        [JsonProperty] public MinMaxValue Focus { get; private set; }

        [JsonConstructor] private ActionPoints() { }

        public ActionPoints(string entityId, string name, ActionPointsTemplate template)
            : base(entityId, name)
        {
            Action = new MinMaxValue(entityId, nameof(Action), template.Action);
            Exertion = new MinMaxValue(entityId, nameof(Exertion), template.Exertion);
            Focus = new MinMaxValue(entityId, nameof(Focus), template.Focus);
        }

        public ActionPoints(string entityId, string name, int action, int exertion, int focus)
            : base(entityId, name)
        {
            Action = new MinMaxValue(entityId, nameof(Action), action);
            Exertion = new MinMaxValue(entityId, nameof(Exertion), exertion);
            Focus = new MinMaxValue(entityId, nameof(Focus), focus);
        }
    }
}
