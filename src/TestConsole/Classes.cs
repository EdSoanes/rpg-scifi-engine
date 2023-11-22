using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Parent
    {
        public string Prop1 { get; set; }
        private string Prop2 { get; set; } = "Bananas";
    }

    public class Child : Parent
    {
        public string Child1 { get; set; }
        private string Child2 { get; set; } = "Custard";
    }
}
