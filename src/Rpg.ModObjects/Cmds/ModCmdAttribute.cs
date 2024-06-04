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
        public string? EnabledWhen { get; set; }
        public string? DisabledWhen { get; set; }
        public string[]? EnabledWhenAll { get; set; }
        public string[]? DisabledWhenAll {  get; set; }
        public string? OutcomeMethod { get; set; }
    }
}
