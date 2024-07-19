using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
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
        [JsonProperty] 
        [Threshold(Min = 0)]
        public int Action { get; private set; }

        [JsonProperty]
        public int CurrentAction { get; private set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        public int Exertion { get; private set; }

        [JsonProperty]
        public int CurrentExertion { get; private set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        public int Focus { get; private set; }

        [JsonProperty]
        public int CurrentFocus { get; private set; }

        [JsonConstructor] private ActionPoints() { }

        public ActionPoints(string name, ActionPointsTemplate template)
            : base(name)
        {
            Action = template.Action;
            Exertion = template.Exertion;
            Focus = template.Focus;
        }

        public ActionPoints(string name, int action, int exertion, int focus)
            : base(name)
        {
            Action = action;
            Exertion = exertion;
            Focus = focus;
        }

        protected override void OnLifecycleStarting()
        {
            base.OnLifecycleStarting();
            this.BaseMod(x => x.CurrentAction, x => x.Action)
                .BaseMod(x => x.CurrentExertion, x => x.Exertion)
                .BaseMod(x => x.CurrentFocus, x => x.Focus);
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
        }
    }
}
