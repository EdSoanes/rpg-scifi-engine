using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Cmds
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModCmdAttribute : Attribute
    {
        public string? EnabledOnState { get; set; }
        public string? DisabledOnState { get; set; }
        public string[]? EnabledOnStates { get; set; }
        public string[]? DisabledOnStates {  get; set; }
        public string? OutcomeMethod { get; set; }
    }
}
