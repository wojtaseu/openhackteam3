using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;

namespace FunctionAppOH
{

    public static class GetRatings
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://txtteam3-test-cosmo.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "XfhHVjjS8jnmk0GLts174Hp680MXUMwkE8ElbyiUME91bG31lYKKTApRUgVs40EcKZfJmPU8rC1legwusywqeA==";

        // The Cosmos client instance
        private static CosmosClient cosmosClient;

        // The database we will create
        private static Database database;

        // The container we will create.
        private static Container container;

        // The name of the database and container we will create
        private static string databaseId = "taskDatabase";
        private static string containerId = "Container1";

        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                });
                log.LogInformation("C# HTTP trigger function processed a request.");

                string userId = req.Query["userId"];

                if (userId == null)
                    return new NotFoundResult();

                var sqlQueryText = "SELECT * FROM c WHERE c.userId = '" + userId + "' ";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

                container = cosmosClient.GetContainer(databaseId, containerId);

                FeedIterator<ratingModel> queryResultSetIterator = cosmosClient.GetContainer(databaseId, containerId).GetItemQueryIterator<ratingModel>(queryDefinition);

                List<ratingModel> getRatings = new List<ratingModel>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ratingModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (ratingModel rating in currentResultSet)
                    {
                        getRatings.Add(rating);
                    }
                }

                var responseMessage = System.Text.Json.JsonSerializer.Serialize(getRatings);

                if (getRatings.Count > 0)
                    return new OkObjectResult(responseMessage);
                else
                    return new OkObjectResult("No Ratings");
            }
            catch(Exception e)
            {
                return new NotFoundResult();
            }
        }
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
