using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public enum InputSource
    {
        Context,
        Player,
        Gamesmaster
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public InputSource InputSource { get; set; } = InputSource.Context;
        public string Param { get; set; }
        public string BindsTo { get; set; }

        public InputAttribute() { }
        public InputAttribute(string param, string bindsTo)
        {
            Param = param;
            BindsTo = bindsTo;
        }

        public InputAttribute(string param, string bindsTo, InputSource inputSource)
        {
            Param = param;
            BindsTo = bindsTo;
            InputSource = inputSource;
        }
    }
}
