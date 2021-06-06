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
        public readonly IExposureDataService _exposureDataService;
        private readonly AddressGeocodeRequest _googleAddressGeocodeRequest;



        public RefreshGeocodeData(IExposureDataService exposureDataService)
        {
            _exposureDataService = exposureDataService;

            _googleAddressGeocodeRequest = new AddressGeocodeRequest();
            _googleAddressGeocodeRequest.Key = GetEnvironmentVariable("GoogleAPIKey");
        }

        [FunctionName("RefreshGeocodeData")]
        public async Task<ActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollectionV3",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")] IEnumerable<GeocodeDataItem> geocodeDBData,
            [CosmosDB(
                databaseName: "geocodeDatabase",
                collectionName: "geocodeCollectionV3",
                ConnectionStringSetting = "CosmosDBConnection", CreateIfNotExists = true)] IAsyncCollector<object> geocodeDocument,
            ILogger log)
        {
            log.LogInformation("Request received to refresh geocode data");

            var discoverDataRecords = await _exposureDataService.GetData();

            List<string> addressesProcessed = new List<string>();
            List<GeocodeDataItem> outputRecords = new List<GeocodeDataItem>();
            List<string> failedAddresses = new List<string>();

            foreach(var ptRecord in discoverDataRecords.Where(d => d.Suburb == "Public Transport"
                                                                || d.SiteTitle.StartsWith("PTV Bus Number")
                                                                || d.SiteTitle.StartsWith("Metro Train")))
            {
                var address = $"{ptRecord.SiteTitle}";
                if(!string.IsNullOrEmpty(ptRecord.SiteStreetAddress))
                    address += $" - {ptRecord.SiteStreetAddress.Trim()}";

                var existingDBRecord = geocodeDBData.FirstOrDefault(d => 
                    d.address == address && 
                    d.exposure_date == ptRecord.ExposureDate &&  
                    d.exposure_time_details == ptRecord.ExposureTimeDetails);
                if(existingDBRecord == null)
                {
                    outputRecords.Add(new GeocodeDataItem
                    {
                        is_public_transport = true,
                        address = address,
                        added_date = ptRecord.AddedDate,
                        title = ptRecord.SiteTitle,
                        exposure_date = ptRecord.ExposureDate,
                        exposure_time_details = ptRecord.ExposureTimeDetails,
                        advice_title = ptRecord.AdviceTitle,
                    }); 
                }
                else if(existingDBRecord.added_date < ptRecord.AddedDate)
                {
                    existingDBRecord.is_public_transport = true;
                    existingDBRecord.added_date = ptRecord.AddedDate;
                    existingDBRecord.title = ptRecord.SiteTitle;
                    existingDBRecord.exposure_date = ptRecord.ExposureDate;
                    existingDBRecord.exposure_time_details = ptRecord.ExposureTimeDetails;
                    existingDBRecord.advice_title = ptRecord.AdviceTitle;

                    outputRecords.Add(existingDBRecord);
                    log.LogInformation($"Updating existing public transport record in DB for {address}");
                }
            }

            foreach(var record in discoverDataRecords.Where(d => d.Suburb != "Public Transport" 
                                                                && !d.SiteTitle.StartsWith("PTV Bus Number")
                                                                && !string.IsNullOrEmpty(d.SiteStreetAddress)))
            {
                string address = $"{record.SiteStreetAddress.Trim()}, {record.Suburb?.Trim()}, {record.SiteState?.Trim()}, AU";
                string title = record.SiteTitle;
                string adviceTitle = record.AdviceTitle;

                if(outputRecords.Any(a => a.address == address) || failedAddresses.Any(f => f == address))
                {
                    log.LogInformation($"Duplicate address in results [{address}]");
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
                    {
                        failedAddresses.Add(address);
                        log.LogInformation($"Failed to find an address for {address}");
                    }
                }
            }
           
            ApplyManualUpdates(outputRecords, geocodeDBData, log);

            foreach(var record in outputRecords)
            {
                await geocodeDocument.AddAsync(new
                {
                    record.is_public_transport,
                    record.id,
                    record.address,
                    added_date = record.added_date,
                    record.title,
                    record.exposure_date,
                    record.exposure_time_details,
                    advice_title = record.advice_title,
                    record.location,
                });
            }


            return (ActionResult)new OkObjectResult(addressesProcessed); // returning the list of addresses processed
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        private void ApplyManualUpdates(List<GeocodeDataItem> outputRecords, IEnumerable<GeocodeDataItem> geocodeDBData, ILogger log) 
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
                    else{
                        log.LogInformation($"Manual update record not found, skipping [{update.address}]");
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
