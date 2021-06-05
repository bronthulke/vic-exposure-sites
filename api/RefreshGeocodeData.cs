using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AWD.VicExposureSites
{
    public class RefreshGeocodeData
    {
        public readonly IExposureData _exposureData;
        private readonly AddressGeocodeRequest _googleAddressGeocodeRequest;



        public RefreshGeocodeData(IExposureData exposureData)
        {
            _exposureData = exposureData;

            _googleAddressGeocodeRequest = new AddressGeocodeRequest();
            _googleAddressGeocodeRequest.Key = "AIzaSyCjL7_Yq4nHc5bLoBhSrmhKXkGTnk0GUa4";        // todo: move this into the local settings

        }

        [FunctionName("RefreshGeocodeData")]
        public async Task<ActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollectionV2",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")] IEnumerable<GeocodeDataItem> geocodeDBData,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollectionV2",
                ConnectionStringSetting = "CosmosDBConnection", CreateIfNotExists = true)] IAsyncCollector<object> geocodeDocument,
            ILogger log)
        {
            log.LogInformation("Request received to refresh geocode data");

            var discoverDataRecords = await _exposureData.GetData();

            List<string> addressesProcessed = new List<string>();
            List<GeocodeDataItem> outputRecords = new List<GeocodeDataItem>();

            foreach(var record in discoverDataRecords)
            {
                string address = $"{record.SiteStreetAddress.Trim()}, {record.Suburb.Trim()}, {record.SiteState.Trim()}, AU";
                string title = record.SiteTitle;
                string adviceTitle = record.AdviceTitle;

                if(outputRecords.Any(a => a.address == address))
                {
                    log.LogInformation($"Duplicate address in results (maybe deal with different times later by returning the marker text) [{address}]");
                    continue;  // have already handled this exact address
                }

                var existingDBRecord = geocodeDBData.FirstOrDefault(d => d.address == address);
                if(existingDBRecord == null || existingDBRecord.added_date < record.AddedDate)
                {
                    _googleAddressGeocodeRequest.Address = address;

                    var response = GoogleApi.GoogleMaps.AddressGeocode.Query(_googleAddressGeocodeRequest);

                    if (response.Results.Any()) 
                    {
                        object location = response.Results.FirstOrDefault().Geometry.Location;
                        var addedDate = record.AddedDate;

                        if(existingDBRecord == null)
                        {
                            outputRecords.Add(new GeocodeDataItem
                            {
                                address = address,
                                added_date = addedDate,
                                title = title,
                                advice_title = adviceTitle,
                                location = new GeocodeDataLocationItem { 
                                    lat =  response.Results.FirstOrDefault().Geometry.Location.Latitude,
                                    lng = response.Results.FirstOrDefault().Geometry.Location.Longitude
                                }
                            }); 
                        }
                        else
                        {
                            existingDBRecord.location = JsonSerializer.Deserialize<GeocodeDataLocationItem>(JsonSerializer.Serialize(location));    // convert type
                            existingDBRecord.title = title;
                            existingDBRecord.advice_title = adviceTitle;
                            existingDBRecord.added_date = addedDate;
                            outputRecords.Add(existingDBRecord);
                            log.LogInformation($"Updating existing record in DB for {address}");
                        }
                        log.LogInformation($"Got geocode data from Google for {response.Results.FirstOrDefault().FormattedAddress} [{response.Results.FirstOrDefault().Geometry.Location.Latitude}, {response.Results.FirstOrDefault().Geometry.Location.Longitude}]");
                    }
                    else
                        log.LogInformation($"Failed to find an address for {address}");
                }
            }
           
            ApplyManualUpdates(outputRecords, geocodeDBData);

            foreach(var record in outputRecords)
            {
                await geocodeDocument.AddAsync(new
                {
                    record.id,
                    record.address,
                    added_date = record.added_date,
                    record.title,
                    advice_title = record.advice_title,
                    record.location,
                });
            }


            return (ActionResult)new OkObjectResult(addressesProcessed); // returning the list of addresses processed
        }

        private void ApplyManualUpdates(List<GeocodeDataItem> outputRecords, IEnumerable<GeocodeDataItem> geocodeDBData) 
        {
            // var contents = File.ReadAllText("ManualDataUpdates.json");
            // var manualUpdates = JsonSerializer.Deserialize<List<GeocodeDataItem>>(contents);
            var manualUpdates = ManualDataUpdates.GetData();
            foreach(var update in manualUpdates)
            {
                if(!outputRecords.Any(a => a.address == update.address))
                {
                    // Find in DB data and add to update data if found.  Ignore if not found at all
                    var existingDBRecord = geocodeDBData.FirstOrDefault(d => d.address == update.address);
                    if(existingDBRecord != null)
                    {
                        if(update.location.lat != 0)
                            existingDBRecord.location.lat = update.location.lat;
        
                        if(update.location.lng != 0)
                            existingDBRecord.location.lng = update.location.lng;
                        
                        if(update.title != null)
                            existingDBRecord.title = update.title;
                        
                        if(update.advice_title != null)
                            existingDBRecord.advice_title = update.advice_title;

                        if(update.added_date != DateTime.MinValue)
                            existingDBRecord.added_date = update.added_date;

                        outputRecords.Add(existingDBRecord);
                    }
                }
                else
                {
                    // exists in handled data, so just update
                    var existingRecord = outputRecords.FirstOrDefault(a => a.address == update.address);
     
                    if(update.location.lat != 0)
                        existingRecord.location.lat = update.location.lat;
     
                    if(update.location.lng != 0)
                        existingRecord.location.lng = update.location.lng;
                    
                    if(update.title != null)
                        existingRecord.title = update.title;
                    
                    if(update.advice_title != null)
                        existingRecord.advice_title = update.advice_title;

                    if(update.added_date != DateTime.MinValue)
                        existingRecord.added_date = update.added_date;
                }
            }
        }
    }
}
