<script>
import $ from "jquery";
import rateLimit from 'function-rate-limit';

export default {
  name: "App",
  data() {
    return {
      map: null,
      mapCenter: { lat: -37.8136, lng: 144.9631 },
      addresses: []
    };
  },
  methods: {
    initMap() {
      this.map = new window.google.maps.Map(document.getElementById('map'), {
        center: this.mapCenter,
        zoom: 8,
      })
    },
    geocodeAddress: rateLimit(40, 1000, function(address) {
      const geocoder = new window.google.maps.Geocoder();
        var existing = this.addresses.find(a => a == address);
        if(!existing) {
          this.addresses.push(address);

          console.log(`Geocoding ${address}`);
          geocoder.geocode({ address: address }, (results, status) => {
            if (status === "OK") {
              // console.log(`Adding marker for ${address}`);
              // resultsMap.setCenter(results[0].geometry.location);
              new window.google.maps.Marker({
                map: this.map,
                position: results[0].geometry.location,
              });

              // await (await fetch(`https://vicexposuresites.azurewebsites.net/api/StoreGeocodeData?code=Pv31aIkVYbSnWGHpkfvdK/pmHdvN99AmmulFbwi930sNRv8AKgZumQ==&address=${address}&lat=${results[0].geometry.location.lat}&lng=${results[0].geometry.location.lng}`)).json();

            } else {
              console.log("Failed geocoding for the following reason: " + status);
            }
          });
        }
        else {
          console.log(`Skipping address ${address}`);
        }
    }),
    loadData() {
      const self = this;

      const discoverData = {
        resource_id: "afb52611-6061-4a2b-9110-74c920bede77", // the resource id
        // limit: 15,
      };

      $.ajax({
        url: "https://discover.data.vic.gov.au/api/3/action/datastore_search",
        data: discoverData,
        cache: true,
        dataType: "jsonp",
        success: function(data) {
          const records = data.result.records;
          
          records.forEach((record) => {
            const address = `${record.Site_streetaddress}, ${record.Suburb}, ${record.Site_state}, AU`;
            self.geocodeAddress(address);
          });
        },
    });
    }
  },
  mounted () {
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
    <button class="btn btn-primary" @click="loadData">Load data</button>

    <div id="map"></div>
  </div>

</template>
