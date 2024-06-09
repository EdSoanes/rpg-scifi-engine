using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MetaPropUIAttribute: Attribute
    {
        public string Name { get => GetType().Name; }
        public string Editor { get; set; }
        public bool Ignore {  get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

        public MetaPropUIAttribute(string editor)
        {
            Editor = editor;
        }
    }

    public class ComponentUIAttribute : MetaPropUIAttribute
    {
        public ComponentUIAttribute()
            : base("Component")
        { }
    }

    public class IntegerUIAttribute : MetaPropUIAttribute
    {
        public string Unit { get; set; } = nameof(Int32);
        public int Min { get; set; } = int.MinValue;
        public int Max { get; set; } = int.MaxValue;
        public string[]? Keys => null;

        public IntegerUIAttribute()
            : base(nameof(Int32))
        { }
    }

    public class MinZeroUIAttribute : IntegerUIAttribute
    {
        public MinZeroUIAttribute()
        {
            Min = 0;
        }
    }

    public class MetersUIAttribute : IntegerUIAttribute
    {
        public MetersUIAttribute() 
        {
            Unit = "m";
        }
    }

    public class AccelerationUIAttribute : IntegerUIAttribute
    {
        public AccelerationUIAttribute()
        {
            Unit = "m/s";
        }
    }

    public class PercentUIAttribute : IntegerUIAttribute
    {
        public PercentUIAttribute()
        {
            Unit = "%";
        }
    }

    public class DiceUIAttribute : MetaPropUIAttribute
    {
        public DiceUIAttribute()
            : base(nameof(Dice))
        { }
    }

    public class TextUIAttribute : MetaPropUIAttribute
    {
        public TextUIAttribute()
            : base(nameof(String))
        { }
    }

    public class RichTextUIAttribute : MetaPropUIAttribute
    {
        public RichTextUIAttribute()
            : base("Html")
        { }
    }
}
