using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Rpg.Sys;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using System.Net;
using System.Text;

namespace Rpg.Api.Functions
{
    public class PUT_Character
    {
        [Function(nameof(PUT_Character))]
        [OpenApiOperation(nameof(PUT_Character))]
        [OpenApiParameter("id", Type = typeof(int), Required = true, In = ParameterLocation.Path)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ActorTemplate), Description = "Character")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "character")] HttpRequest req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var humanTemplate = JsonConvert.DeserializeObject<ActorTemplate>(json);
            if (humanTemplate == null)
                return new BadRequestResult();

            var graph = CreateHumanGraph(humanTemplate);

            var res = await UploadCharacterAsync(graph);

            return new ContentResult
            {
                Content = graph.Serialize<Human>(),
                ContentType = "application/json"
            };
        }

        public Graph CreateHumanGraph(ActorTemplate actorTemplate)
        {
            var graph = new Graph();
            var equipment = new Equipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            var human = new Human(actorTemplate);

            human.RightHand.Add(equipment);
            graph.SetContext(human);

            return graph;
        }

        private async Task<Azure.Response<BlobContentInfo>> UploadCharacterAsync(Graph graph)
        {
            string connection = AzFunc.Common.AppSettings.StorageConnectionString;// Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = "characters";

            var json = graph.Serialize<Human>();

            using var myBlob = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var blobClient = new BlobContainerClient(connection, containerName);
            var blob = blobClient.GetBlobClient(graph.GetContext<Human>().Id.ToString());

            return await blob.UploadAsync(myBlob);
        }
    }
}
