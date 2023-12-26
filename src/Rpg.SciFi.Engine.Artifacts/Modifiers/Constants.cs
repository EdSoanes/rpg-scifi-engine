using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public static class RemoveTurn
    {
        public const int WhenZero = -2;
        public const int Encounter = -1;
        public const int This = 0;
    }

    public static class ModNames
    {
        public const string Base = "Base";
        public const string Damage = "Damage";
        public const string Cost = "Cost";
        public const string Turn = "Turn";
    }

    public enum ModifierType
    {
        Base,
        Player,
        Custom,
        Transient,
        State
    }

    public enum ModifierAction
    {
        Replace,
        Sum,
        Accumulate
    }
}
