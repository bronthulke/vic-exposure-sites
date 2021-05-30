using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AWD.GetAllGeocodeData
{
    public class GeocodeDataItem
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("location")]
        public GeocodeDataLocationItem location { get; set; }

    }

    public class GeocodeDataLocationItem
    {
        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lng")]
        public string Lng { get; set; }

    }

    public static class GetAllGeocodeData
    {
        
        [FunctionName("GetAllGeocodeData")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollection",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")] IEnumerable<GeocodeDataItem> geocodeData,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(geocodeData);
        }
    }
}
