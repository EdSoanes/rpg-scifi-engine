﻿using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    public class VirtualPropTests
    {
        [SetUp]
        public void Setup()
        {
            RpgGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test] 
        public void AddVirtualProp_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddMod(new PermanentMod(), "VirtualProp", 1);
            graph.Time.TriggerEvent();

            Assert.That(graph.GetPropValue(entity, "VirtualProp").Roll(), Is.EqualTo(1));
        }
    }
}