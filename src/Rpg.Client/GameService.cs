using Rpg.Sys;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Moddable;

namespace Rpg.Client
{
    public class GameService<T> : IGameService<T> where T : ModObject
    {
        public Graph Graph { get; private set; }
        public ActorTemplate[] Humans { get; private set; }

        public GameService()
        {
            Graph = new Graph();
            Humans = GetActors();
        }

        public void SetContext(T context)
        {
            Graph.SetContext(context);
        }

        private ActorTemplate[] GetActors()
        {
            return new[]
            {
                new ActorTemplate
                {
                    Name = "Bartleby Blaze",
                    Class = "Warrior",
                    Description = "A deadly warrior from the mean streets of Lima Kilo",
                    Stats = new StatPointsTemplate
                    {
                        Strength = 18,
                        Intelligence = 8,
                        Wisdom = 4,
                        Dexterity = 15,
                        Constitution = 16,
                        Charisma = 10
                    },
                    Actions = new ActionPointsTemplate
                    {
                        Action = 10,
                        Exertion = 10,
                        Focus = 10,
                    },
                    Health = new HealthTemplate
                    {
                        Physical = 8,
                        Mental = 8
                    },
                    Presence = new PresenceTemplate
                    {
                        HeatCurrent = 36,
                        Size = 3,
                        Weight = 100
                    }
                },
                new ActorTemplate
                {
                    Name = "Felomina Hond",
                    Class = "Hacker",
                    Description = "Born to wealthy corp careerists, she hates her old life",
                    Stats = new StatPointsTemplate
                    {
                        Strength = 9,
                        Intelligence = 18,
                        Wisdom = 12,
                        Dexterity = 15,
                        Constitution = 4,
                        Charisma = 13
                    },
                    Actions = new ActionPointsTemplate
                    {
                        Action = 10,
                        Exertion = 10,
                        Focus = 10,
                    },
                    Health = new HealthTemplate
                    {
                        Physical = 8,
                        Mental = 8
                    },
                    Presence = new PresenceTemplate
                    {
                        HeatCurrent = 36,
                        Size = 3,
                        Weight = 65
                    }
                },
                new ActorTemplate
                {
                    Name = "Horatio Thumblethorp",
                    Class = "Medic",
                    Description = "An ex-military field medic, he learned to hate violence and the people who start war",
                    Stats = new StatPointsTemplate
                    {
                        Strength = 8,
                        Intelligence = 15,
                        Wisdom = 17,
                        Dexterity = 13,
                        Constitution = 10,
                        Charisma = 10
                    },
                    Actions = new ActionPointsTemplate
                    {
                        Action = 10,
                        Exertion = 10,
                        Focus = 10,
                    },
                    Health = new HealthTemplate
                    {
                        Physical = 8,
                        Mental = 8
                    },
                    Presence = new PresenceTemplate
                    {
                        HeatCurrent = 36,
                        Size = 3,
                        Weight = 85
                    }
                }
            };
        }
    }
}
