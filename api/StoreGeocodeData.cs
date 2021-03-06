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

namespace AWD.VicExposureSites
{
    class StoreGeocodeDataRequestData {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("title")]
        public string Title { get; set;}

        [JsonProperty("advice_title")]
        public string AdviceTitle { get; set;}

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
                collectionName: "geocodeCollectionV2",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")] IEnumerable<GeocodeDataItem> geocodeData,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollectionV2",
                ConnectionStringSetting = "CosmosDBConnection", Id = "address", CreateIfNotExists = true)] out object geocodeDocument,
            ILogger log)
        {
            throw new NotImplementedException("No longer supported");

            // string requestBody = new StreamReader(req.Body).ReadToEnd();
            // var data = JsonConvert.DeserializeObject<StoreGeocodeDataRequestData>(requestBody);

            // string address = data?.Address;
            // string title = data?.Title;
            // string adviceTitle = data?.AdviceTitle;
            // object location = data?.Location;

            // // Ensure we don't try to add duplicate addresses (which would throw an exception, since "/address" is a unique key)
            // if(geocodeData.Any(d => d.address == address))
            // {
            //     Console.WriteLine("Not adding existing item " + address);
            //     geocodeDocument = null;
            //     return (ActionResult)new OkResult();
            // }            

            // // We need both name and task parameters.
            // if (!string.IsNullOrEmpty(address) && location != null)
            // {
            //     geocodeDocument = new
            //     {
            //         address,
            //         title,
            //         adviceTitle,
            //         location,
            //     }; 

            //     return (ActionResult)new OkObjectResult(geocodeDocument); // returning the doc JSON
            // }
            // else
            // {
            //     geocodeDocument = null;
            //     return (ActionResult)new BadRequestResult();
            // }
        }
    }
}
