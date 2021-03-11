using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Microsoft.Azure.Documents;

namespace FunctionAppOH
{
    public static class GetRating
    {

        [FunctionName("GetRating")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "rating/{ratingId}")] HttpRequest req,
        [CosmosDB(
        databaseName: "taskDatabase",
        collectionName: "Container1",
        Id = "{ratingId}",
        PartitionKey = "{ratingId}",
        ConnectionStringSetting = "CosmosDBConnection")] Document document,
        ILogger log)
        {
            return document != null
            ? (ActionResult)new OkObjectResult(document)
            : new BadRequestObjectResult("Please revisit your route parameters");
        }



        // return new OkResult();

        // return new OkObjectResult();

    



        internal class ratingModel
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string productId { get; set; }
            public DateTime timestamp { get; set; }
            public string locationName { get; set; }
            public int rating { get; set; }
            public string userNotes { get; set; }
        }

    }


}

