<script>
import $ from "jquery";
import rateLimit from 'function-rate-limit';
import Utilities from './utilities'

export default {
  name: "App",
  data() {
    return {
      map: null,
      mapCenter: { lat: -37.8136, lng: 144.9631 },
      geocoder: null,
      addresses: [],
      localCache: [],
      exposureDataBaseURL: "https://discover.data.vic.gov.au"
    };
  },
  methods: {
    initMap() {
      this.map = new window.google.maps.Map(document.getElementById('map'), {
        center: this.mapCenter,
        zoom: 8,
      })
    },
    callGetAllGeocodeData() {
      const url = `http://192.168.1.178:4280/api/getallgeocodedata/`;

      const options = {
        method: "GET",
        cache: "no-cache",
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      };

      return fetch(url, options)
        .then(Utilities.handleAPIErrors)
        .then(r =>  r.json().then(data => ({status: r.status, body: data})))
        .then(data => {
          // console.log(`Stored data - ${data.body}`);
          this.localCache = data.body;
        });
    },
    callStoreGeocodeDataAPI(address, geometryLocation) {
        const url = `http://192.168.1.178:4280/api/storegeocodedata/?address=${address}&lat=${geometryLocation.lat}&lng=${geometryLocation.lng}`;

        const data = {
          "address": address,
          "location": geometryLocation
        };

        const options = {
            method: "POST",
            cache: "no-cache",
            body: JSON.stringify(data),
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json'
            }
        };

        return fetch(url, options)
            .then(Utilities.handleAPIErrors)
            .then(r =>  r.json().then(data => ({status: r.status, body: data})))
            .then(data => {
              return data.body;
            });
    },
    getCachedGeocode(address) {
        /// TODO: Create this function that gets the cached geocode data for an address.
        /// Nb: need to consider that another request may have already updated the DB with it it since we got that data
        /// into our local cache
        // console.log(`Getting address ${address} from local cache`)
        var localCacheData = this.localCache.find(i => i.address == address);
        
        return localCacheData;
    },
    lookupGeocodeWithGoogle: rateLimit(10, 1000, function(address) { 

      // console.log(`Geocoding ${address}`);
      this.geocoder.geocode({ address: address }, (results, status) => {
        if (status === "OK") {
          // console.log(`Adding marker for ${address}`);
          // resultsMap.setCenter(results[0].geometry.location);
          new window.google.maps.Marker({
            map: this.map,
            position: results[0].geometry.location,
          });

          this.callStoreGeocodeDataAPI(address, results[0].geometry.location)
            .then(newData => {
              this.localCache.push(newData);
            })
            .catch((error) => {
              console.log(error);
            });

          this.addresses.push(address);   // record as having been processed

        } else {
          console.log("Failed geocoding for the following reason: " + status);
        }
      });
    }),
    geocodeAddress(address) { 
      // NOTE: I am NOT putting this initial geocoding stuff into a separate Azure Function for now,
      // so the first time this loads with an empty database, it will HAVE to run slowly
      // so as not to run into Google API rate limits.  So I'll be sure to run this myself first.
      // ALSO: because of the weird Google rate limit issues (50QPS is NOT working), I may need
      // to run it a few times in order to get all the data loaded.

      var processedAddress = this.addresses.find(a => a == address);
        if(!processedAddress) {

        var geocodeData = this.getCachedGeocode(address);
        if(geocodeData) {
          // console.log(`Found local cache data for ${address}: ${JSON.stringify(geocodeData)}`)
          
          var latLng = { lat: Number(geocodeData.location.lat), lng: Number(geocodeData.location.lng)};
          
          new window.google.maps.Marker({
            map: this.map,
            position: latLng,
          });

          this.addresses.push(address);   // record as having been processed
        }
        else {
          this.lookupGeocodeWithGoogle(address);
        }
      }
      else {
        // console.log(`Skipping address already mapped ${address}`);
      }
    },
    doLoadData() {
      this.loadData("/api/3/action/datastore_search?resource_id=afb52611-6061-4a2b-9110-74c920bede77");
    },
    loadData(url) {
      const self = this;

      $.ajax({
        url: `${this.exposureDataBaseURL}${url}`,
        cache: true,
        dataType: "jsonp",
        success: function(data) {
          const records = data.result.records;

          records.forEach((record) => {
            const address = `${record.Site_streetaddress}, ${record.Suburb}, ${record.Site_state}, AU`;
            self.geocodeAddress(address);
          });

          if(data.result.records.length > 0) {
            self.loadData(data.result._links.next)
          }
        },
    });
    }
  },
  mounted () {
    
    this.callGetAllGeocodeData();

    this.geocoder = new window.google.maps.Geocoder();

    this.initMap();
  },
};
</script>

<style scoped>
#map {
  height: 700px;
}
</style>

<template>
  <div class="container">
    <h2>Vic Exposure Sites</h2>
    <button class="btn btn-primary" @click="doLoadData">Load data</button>

    <p><strong>Coming soon:</strong> classification of Tier 1/Tier 2/Tier 3</p>

    <div id="map"></div>
    <div id="disclaimer">
      <p>Created by Bron Thulke - code available at <a href="https://github.com/bronthulke/radius-checker">https://github.com/bronthulke/vic-exposure-sites</a></p>
      <p><strong>DISCLAIMER:</strong> The data presented in this website is not guaranteed to be accurate, and we take no responsibility for any decisions or outcomes that come about as a result of the use of this website.
      <p>This project is a "passion project" by me, in order to allow me to visually see any nearby COVID-19 exposure sites in Melbourne.</p>
      <p>The data may be out of date, and may become out-of-date as the government updates data on a regular basis.</p>
    </div>
  </div>
</template>
