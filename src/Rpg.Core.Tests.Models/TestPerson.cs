using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Core.Tests.Models
{
    public class TestPerson : RpgEntity
    {
        [Threshold(Min = 3, Max = 18)]
        public int Strength { get; set; } = 13;
        public int StrengthBonus { get; set; }
        public int MeleeAttack { get; set; } = 1;
        public int MeleeDefence { get; set; } = 10;
        public int HitPoints { get; set; } = 10;

        public RpgObjectCollection Hands { get; set; }
        public RpgObjectCollection Wearing { get; set; }

        public TestEnhancement? Enhancement
        {
            get => GetChildObject<TestEnhancement>(nameof(Enhancement));
            set => AddChild(nameof(Enhancement), value);
        }

        [JsonConstructor] protected TestPerson() { }

        public TestPerson(string name)
            : base(name)
        {
            Hands = new RpgObjectCollection(Id, nameof(Hands));
            Wearing = new RpgObjectCollection(Id, nameof(Wearing));
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Hands.OnCreating(graph, entity);
            Wearing.OnCreating(graph, entity);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Hands.OnTimeBegins();
            Wearing.OnTimeBegins();

            this
                .AddMod(new Base(), x => x.StrengthBonus, x => x.Strength, () => DiceCalculations.CalculateStatBonus)
                .AddMod(new Base(), x => x.MeleeAttack, x => x.StrengthBonus);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity)
        {
            base.OnRestoring(graph, entity);
            Hands.OnRestoring(graph, entity);
            Wearing.OnRestoring(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            Hands.OnStartLifecycle();
            Wearing.OnStartLifecycle();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            Hands.OnUpdateLifecycle();
            Wearing.OnUpdateLifecycle();
            return Expiry;
        }
    }
}
