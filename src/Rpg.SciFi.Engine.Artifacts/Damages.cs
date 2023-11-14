using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class BaseDamage
    {
        public BaseDamage() { }

        public BaseDamage(string name, string description, Dice dice)
        {
            Name = name;
            Description = description;
            Dice = dice;
        }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; private set; }
        [JsonProperty] public Dice Dice { get; private set; } = "d6";
    }

    public class Damage : Modifiable<BaseDamage>
    {
        public Damage() { }
        public Damage(string name, string description, Dice dice)
        {
            BaseModel = new BaseDamage(name, description, dice);
        }

        public Guid Id => BaseModel.Id;
        public string Name => BaseModel.Name;
        public string? Description => BaseModel.Description;
        public Dice BaseDice => BaseModel.Dice;
        public Dice Dice => BaseDice + ModifierDice("Value");
    }

    public class DamageSignature
    {
        [JsonProperty] public Damage Impact { get; protected set; } = new Damage();
        [JsonProperty] public Damage Pierce { get; protected set; } = new Damage();
        [JsonProperty] public Damage Blast { get; protected set; } = new Damage();
        [JsonProperty] public Damage Burn { get; protected set; } = new Damage();
        [JsonProperty] public Damage Energy { get; protected set; } = new Damage();
    }
}
