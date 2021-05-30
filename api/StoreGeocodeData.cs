using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using AWD.GetAllGeocodeData;

namespace AWD.StoreGeocodeData
{
    class RequestData{
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("location")]
        public object Location { get; set; }
    }

    public static class StoreGeocodeData
    {
        [FunctionName("StoreGeocodeData")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollection",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")] IEnumerable<GeocodeDataItem> geocodeData,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollection",
                ConnectionStringSetting = "CosmosDBConnection", Id = "address", CreateIfNotExists = true)] out object geocodeDocument,
            ILogger log)
        {

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            RequestData data = JsonConvert.DeserializeObject<RequestData>(requestBody);

            string address = data?.Address;
            object location = data?.Location;

            // Ensure we don't try to add duplicate addresses (which would throw an exception, since "/address" is a unique key)
            if(geocodeData.Any(d => d.Address == address))
            {
                Console.WriteLine("Not adding existing item " + address);
                geocodeDocument = null;
                return (ActionResult)new OkResult();
            }            

            // We need both name and task parameters.
            if (!string.IsNullOrEmpty(address) && location != null)
            {
                geocodeDocument = new
                {
                    address,
                    location,
                }; 

                return (ActionResult)new OkObjectResult(geocodeDocument); // returning the doc JSON
            }
            else
            {
                geocodeDocument = null;
                return (ActionResult)new BadRequestResult();
            }
        }
    }
}
