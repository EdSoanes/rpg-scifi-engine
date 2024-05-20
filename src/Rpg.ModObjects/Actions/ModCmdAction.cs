using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ModCmdAction
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(ModCmdAction);

        public ModCmdAction(string name)
            => Name = name;
    }
}
