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
    public class ActionPoints : RpgEntityComponent
    {
        [JsonProperty] public MaxCurrentValue Action { get; private set; }
        [JsonProperty] public MaxCurrentValue Exertion { get; private set; }
        [JsonProperty] public MaxCurrentValue Focus { get; private set; }

        [JsonConstructor] private ActionPoints() { }

        public ActionPoints(string entityId, string name, ActionPointsTemplate template)
            : base(entityId, name)
        {
            Action = new MaxCurrentValue(entityId, nameof(Action), template.Action);
            Exertion = new MaxCurrentValue(entityId, nameof(Exertion), template.Exertion);
            Focus = new MaxCurrentValue(entityId, nameof(Focus), template.Focus);
        }

        public ActionPoints(string entityId, string name, int action, int exertion, int focus)
            : base(entityId, name)
        {
            Action = new MaxCurrentValue(entityId, nameof(Action), action);
            Exertion = new MaxCurrentValue(entityId, nameof(Exertion), exertion);
            Focus = new MaxCurrentValue(entityId, nameof(Focus), focus);
        }
    }
}
