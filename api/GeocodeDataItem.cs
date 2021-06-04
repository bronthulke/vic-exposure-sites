using System;
using System.Text.Json.Serialization;

namespace AWD.VicExposureSites
{

    public class GeocodeDataItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("address")]
        public string address { get; set; }

        [JsonPropertyName("title")]
        public string title { get; set;}

        [JsonPropertyName("advice_title")]
        public string advice_title { get; set;}

        [JsonPropertyName("added_date")]
        public DateTime added_date { get; set;}

        [JsonPropertyName("location")]
        public GeocodeDataLocationItem location { get; set; }

    }

    public class GeocodeDataLocationItem
    {
        [JsonPropertyName("lat")]
        public double lat { get; set; }

        [JsonPropertyName("lng")]
        public double lng { get; set; }

    }

}