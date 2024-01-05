using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class ActionPoints
    {
        [JsonProperty] public MaxCurrentValue Action { get; private set; }
        [JsonProperty] public MaxCurrentValue Exertion { get; private set; }
        [JsonProperty] public MaxCurrentValue Focus { get; private set; }

        [JsonConstructor] private ActionPoints() { }

        public ActionPoints(ActionPointsTemplate template)
        {
            Action = new MaxCurrentValue(template.Action, template.Action);
            Exertion = new MaxCurrentValue(template.Exertion, template.Exertion);
            Focus = new MaxCurrentValue(template.Focus, template.Focus);
        }

        public ActionPoints(int action, int exertion, int focus)
        {
            Action = new MaxCurrentValue(action, action);
            Exertion = new MaxCurrentValue(exertion, exertion);
            Focus = new MaxCurrentValue(focus, focus);
        }
    }
}
