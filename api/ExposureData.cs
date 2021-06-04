using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AWD.VicExposureSites
{
    public class DiscoverData
    {
        [JsonPropertyName("result")]
        public DiscoverDataResult Results { get; set;}
    }
    public class DiscoverDataResult
    {
        [JsonPropertyName("records")]
        public List<DiscoverDataRecord> Records { get; set;}

        [JsonPropertyName("_links")]
        public Link Links { get;set;}

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class DiscoverDataRecord
    {
        [JsonPropertyName("_id")]
        public int Id { get; set;}

        [JsonPropertyName("Suburb")]
        public string Suburb { get; set;}

        [JsonPropertyName("Site_title")]
        public string SiteTitle { get; set;}

        [JsonPropertyName("Advice_title")]
        public string AdviceTitle { get; set;}

        [JsonPropertyName("Site_streetaddress")]
        public string SiteStreetAddress { get; set;}

        [JsonPropertyName("Site_state")]
        public string SiteState { get; set;}

        [JsonPropertyName("Site_postcode")]
        public string SitePostcode { get; set;}
        
        [JsonPropertyName("Added_date_dtm")]
        public DateTime AddedDate { get; set;}
        // Other fields exist if needed
        
    }

    public class Link 
    {
        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }
    }

    public class ExposureData : IExposureData
    {
        HttpClient _client;
        List<DiscoverDataRecord> _allRecords;
        private readonly string _exposureDataBaseURL = "https://discover.data.vic.gov.au";

        public ExposureData(HttpClient client)
        {
            _client = client;
            _allRecords = new List<DiscoverDataRecord>();
        }

        public async Task<List<DiscoverDataRecord>> GetData()
        {
            await LoadDiscoverData("/api/3/action/datastore_search?resource_id=afb52611-6061-4a2b-9110-74c920bede77");

            return _allRecords;
        }

        public async Task LoadDiscoverData(string path) {

            using var responseStream = await _client.GetStreamAsync(GetExposureSiteDataURL(path));
            var discoverData = await JsonSerializer.DeserializeAsync<DiscoverData>(responseStream);

            if(discoverData.Results.Records.Count > 0)
            {
                _allRecords.AddRange(discoverData.Results.Records);
                await LoadDiscoverData(discoverData.Results.Links.Next);
            }
        }

        private string GetExposureSiteDataURL(string url)
        {
            return _exposureDataBaseURL + url;
        }
    }
}