<script>
import $ from "jquery";
import rateLimit from 'function-rate-limit';
import Utilities from './utilities'

export default {
  name: "App",
  data() {
    return {
      map: null,
      cacheInitialised: false,
      mapCenter: { lat: -37.8136, lng: 144.9631 },
      infoWindow: undefined,
      oms: [],
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

      this.infoWindow = new window.google.maps.InfoWindow();

      this.oms = new window.OverlappingMarkerSpiderfier(this.map, {
        keepSpiderfied: true,
      });

    },
    callGetAllGeocodeData() {
      const url = `/api/getallgeocodedata/`;
      // const url = `http://192.168.1.178:4280/api/getallgeocodedata/`;

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
          this.cacheInitialised = true;
        });
    },
    callStoreGeocodeDataAPI(address, geometryLocation, title, adviceTitle) {
        const url = `/api/storegeocodedata/`;
        // const url = `http://192.168.1.178:4280/api/storegeocodedata/`;

        const data = {
          "address": address,
          "location": geometryLocation,
          "title": title,
          "advice_title": adviceTitle
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
    addMarkerAndInfoWindow(address, title, adviceTitle, latLng) {

      let iconURL = '';
      if(adviceTitle.startsWith('Tier 2'))
        iconURL = 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png';
      else if(adviceTitle.startsWith('Tier 3'))
        iconURL = 'https://maps.google.com/mapfiles/ms/icons/green-dot.png';
      else // Tier 1 and anything else that can't be classified
        iconURL = 'https://maps.google.com/mapfiles/ms/icons/red-dot.png';

      var marker = new window.google.maps.Marker({
        // map: this.map, // using Spiderify
        position: latLng,
        title: title,
        icon: iconURL
      });

      // this.infoWindow = new window.google.maps.InfoWindow({
      //   content: `<p><strong>${title}</strong></p>` +
      //     `<p>${adviceTitle}</></p>` +
      //     `<p>${address}</></p>`
      // });

      // marker.addListener("click", () => {
      //   this.infoWindow.close();
      //   this.infoWindow.open(this.map, marker);
      // });

      let iw = this.infoWindow;
      window.google.maps.event.addListener(marker, 'spider_click', () => {
        if(iw) iw.close();

        iw.setContent(`<p><strong>${title}</strong></p>` +
          `<p>${adviceTitle}</></p>` +
          `<p>${address}</></p>`);
        iw.open(this.map, marker);
      });
      this.oms.addMarker(marker);



    },
    lookupGeocodeWithGoogle: rateLimit(10, 1000, function(address, title, adviceTitle) { 

      console.log(`Geocoding ${address}`);
      this.geocoder.geocode({ address: address }, (results, status) => {
        if (status === "OK") {

          this.addMarkerAndInfoWindow(address, title, adviceTitle, results[0].geometry.location);

          this.callStoreGeocodeDataAPI(address, results[0].geometry.location, title, adviceTitle)
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
    geocodeAddress(address, title, adviceTitle) { 
      // NOTE: I am NOT putting this initial geocoding stuff into a separate Azure Function for now,
      // so the first time this loads with an empty database, it will HAVE to run slowly
      // so as not to run into Google API rate limits.  So I'll be sure to run this myself first.
      // ALSO: because of the weird Google rate limit issues (50QPS is NOT working), I may need
      // to run it a few times in order to get all the data loaded.

      var processedAddress = this.addresses.find(a => a == address);
        if(!processedAddress) {

        var geocodeData = this.getCachedGeocode(address);
        if(geocodeData) {
          console.log(`Found local cache data for ${address}: ${JSON.stringify(geocodeData)}`)
          
          var latLng = { lat: Number(geocodeData.location.lat), lng: Number(geocodeData.location.lng)};
          
          this.addMarkerAndInfoWindow(address, title, adviceTitle, latLng);

          this.addresses.push(address);   // record as having been processed
        }
        else {
          this.lookupGeocodeWithGoogle(address, title, adviceTitle);
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
            self.geocodeAddress(address, record.Site_title, record.Advice_title);
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
  .legend img {
    margin-bottom: 5px;
  }
  #map {
    height: 700px;
  }
</style>

<template>
  <div class="container">
    <h2>Victorian COVID-19 Public Exposure Sites</h2>
    <div class="row">
      <div class="col-md-8">
        <p><button class="btn btn-primary" :disabled="!cacheInitialised" @click="doLoadData">Load data <span v-show="!cacheInitialised">(please wait)</span></button></p>

        <p>For full details of COVID-19 exposure sites, go to <a href="https://www.coronavirus.vic.gov.au/exposure-sites">https://www.coronavirus.vic.gov.au/exposure-sites</a></p>
      </div>
      <div class="col-md-4">
        <p class="legend"><strong>Legend:</strong><br/>
          <img src="https://maps.google.com/mapfiles/ms/icons/red-dot.png" /> Tier 1<br/>
          <img src="https://maps.google.com/mapfiles/ms/icons/blue-dot.png" /> Tier 2<br/>
          <img src="https://maps.google.com/mapfiles/ms/icons/green-dot.png" /> Tier 3
        </p>
      </div>
    </div>
    <div id="map"></div>
    <div class="row">
      <div class="col-xs-12">
        <div id="disclaimer">
          <p>Created by Bron Thulke - code available on <a href="https://github.com/bronthulke/vic-exposure-sites">Github</a></p>
          <p><strong>DISCLAIMER:</strong> The data presented in this website is not guaranteed to be accurate, and we take no responsibility for any decisions or outcomes that come about as a result of the use of this website.
          <p>This project is a "passion project" by me, in order to allow me to visually see any nearby COVID-19 exposure sites in Melbourne.</p>
          <p>The data may be out of date, and may become out-of-date as the government updates data on a regular basis.</p>
        </div>
      </div>
    </div>
  </div>
</template>
