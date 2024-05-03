using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Rpg.Sys;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using System.Net;

namespace Rpg.Api.Functions
{
    public class GET_Character
    {
        [Function(nameof(GET_Character))]
        [OpenApiOperation(nameof(GET_Character))]
        [OpenApiParameter("id", Type = typeof(int), Required = true, In = ParameterLocation.Path)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GraphState<Human>), Description = "Character")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "character/{id}")] HttpRequest req, Guid id)
        {
            var graph = CreateHumanGraph();
            return new ContentResult
            {
                Content = graph.Serialize<Human>(),
                ContentType = "application/json"
            };
        }

        public Graph CreateHumanGraph()
        {
            var graph = new Graph();
            var equipment = new Equipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            var human = new Human(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                },
                Presence = new PresenceTemplate
                {
                    Weight = 80,
                    HeatMax = 36,
                    HeatCurrent = 36
                }
            });

            human.RightHand.Add(equipment);
            graph.SetContext(human);

            return graph;
        }
    }
}
