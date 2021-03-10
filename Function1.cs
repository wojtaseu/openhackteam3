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

namespace FunctionAppOH
{
    public static class Function1
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
        private static string containerId = "container1";

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                });

                var sqlQueryText = "SELECT * FROM c ";


                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<test> queryResultSetIterator = cosmosClient.GetContainer(databaseId,containerId).GetItemQueryIterator<test>(queryDefinition);

                List<test> families = new List<test>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<test> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (test family in currentResultSet)
                    {
                        families.Add(family);
                    }
                }


                log.LogInformation("C# HTTP trigger function processed a request.");

                string productId = "";
                string userId = "";
                int? rating = null;
                string errorText = "";

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                
                productId = data?.productId;
                userId = data?.userId;
                rating = data?.rating;


                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage responseProduct = await client.GetAsync("https://serverlessohproduct.trafficmanager.net/api/GetProduct?productId="+ productId);
                    responseProduct.EnsureSuccessStatusCode();
                    string responseBody = await responseProduct.Content.ReadAsStringAsync();
                
                    Product getProduct = System.Text.Json.JsonSerializer.Deserialize<Product>(responseBody);
                }
                catch(Exception ex)
                {
                    errorText += "1";
                    log.LogInformation("product error: " + ex.ToString());
                }

                try
                {
                    HttpClient client2 = new HttpClient();
                    HttpResponseMessage responseUSer = await client2.GetAsync("https://serverlessohuser.trafficmanager.net/api/GetUser?userId=" + userId);
                    responseUSer.EnsureSuccessStatusCode();
                    string responseBody2 = await responseUSer.Content.ReadAsStringAsync();
                
                    User getUser = System.Text.Json.JsonSerializer.Deserialize<User>(responseBody2);
                }
                catch (Exception ex)
                {
                    errorText += "2";
                    log.LogInformation("user error: " + ex.ToString());
                }

                if(0 < rating || 5 > rating)
                {
                    errorText += "3";
                    log.LogInformation("invalid rating");
                }

                if(errorText.Length == 0)
                {
                    string guid = Guid.NewGuid().ToString();
                    DateTime dt = DateTime.UtcNow;
                }


                string responseMessage = string.IsNullOrEmpty(productId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : req.Method.Equals("GET") ? $"The product name for your product id {productId} is Starfruit Explosion" : $"The product name for your product id {productId} is Starfruit Explosion and the description is This starfruit ice cream is out of this world!";

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                return new OkObjectResult("nada");
            }
        }
    }

    //private static async Task<List<Product>> GetProductsAsync()
    //{
    //}
    //private static Product GetProduct(string productid)
    //{

    //}

    //{"productId":"75542e38-563f-436f-adeb-f426f1dabb5c","productName":"Starfruit Explosion","productDescription":"This starfruit ice cream is out of this world!"}
    internal class Product{
        public string productId { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
    }

    //{"userId":"cc20a6fb-a91f-4192-874d-132493685376","userName":"doreen.riddle","fullName":"Doreen Riddle"}
    internal class User
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string fullName { get; set; }
    }

    internal class test
    {
        public string name { get; set; }
        public string duedate { get; set; }
        public string task { get; set; }
        public string id { get; set; }
    }
}
