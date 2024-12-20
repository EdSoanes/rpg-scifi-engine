﻿using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public abstract class Time : Mod
    {
        [JsonConstructor] protected Time()
            : base() { }

        protected Time(string name, Lifespan lifespan) 
            : base(name)
        {
            Lifespan = lifespan;
            Behavior = new Add();
        }

        protected Time(string name, PointInTimeType start, PointInTimeType end)
            : this(name, new Lifespan(start, end))
        { }

        protected Time(string name, int startTurn, int duration)
            : this(name, new Lifespan(startTurn, duration))
        { }

        protected Time(string ownerId, string name, Lifespan lifespan)
            : this(name, lifespan)
        {
            OwnerId = ownerId;
        }

        protected Time(string ownerId, string name, PointInTimeType start, PointInTimeType end)
            : this(name, new Lifespan(start, end))
        {
            OwnerId = ownerId;
        }

        protected Time(string ownerId, string name, int startTurn, int duration)
            : this(name, new Lifespan(startTurn, duration))
        {
            OwnerId = ownerId;
        }
    }
}
