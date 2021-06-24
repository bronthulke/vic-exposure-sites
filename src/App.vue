<script>
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
      localCache: [],
      ptRecords: [],
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
    doMapDataLoad() {
      if(this.oms)
        this.oms.removeAllMarkers();

        this.localCache.forEach((record) => {
          if(record.location && !record.is_public_transport)
          {
            var latLng = { lat: Number(record.location.lat), lng: Number(record.location.lng)};
              
            this.addMarkerAndInfoWindow(record.address, record.title, record.advice_title, latLng);
          }
        });

        this.localCache.filter(r => r.is_public_transport).forEach((ptRecord) => {
          this.ptRecords.push(ptRecord);
        });

        this.ptRecords.sort((a, b) => {
          if(a.address > b.address)
            return 1;
          else if(a.address < b.address)
            return -1;

          if(new Date(a.exposure_date) > new Date(b.exposure_date))
           return 1;
          
          return -1;
        });
    },
    formatDateDMY(item) {
      var date = new Date(item.exposure_date);
      var d = date.getDate();
      var m = date.getMonth() + 1; //Month from 0 to 11
      var y = date.getFullYear();
      return `${d}/${(m<=9 ? '0' + m : m) }/${y}`;
    },
  },
  mounted () {

    this.geocoder = new window.google.maps.Geocoder();
    this.initMap();

    this.callGetAllGeocodeData()
      .then(() => {
        this.doMapDataLoad()
      });

  },
};
</script>

<style scoped>

  h1 {
    margin: 30px 0 20px;
  }

  .legend-items {
    display: flex;
    align-items: center;
    flex-direction: column;
    margin-bottom: 10px;
  }

    @media(max-width: 768px) {
      .legend-items {
        flex-direction: row;
      }
    }
  
    .legend-items .group {
      display: flex;
      flex-direction: row;
      margin-bottom: 10px;
    }

      @media(max-width: 768px) {
        .legend-items .group {
          margin-bottom: 0;
        }
      }

    .legend-items .btn {
        margin-left: auto;
        width: 100%;
    }

    @media(max-width: 767px) {
      .legend-items .btn {
        margin-left: auto;
        width: auto;
      }
    }
        
    .legend-item .text {
        margin-right: 20px;
        white-space: nowrap;
    }

  .map-row {
    margin-bottom: 20px;
  }

  #map {
    height: 700px;
  }

  .spinner{
    position:absolute;
    top: 50%;
    left: 0;
    /* background: #2a2a2a55; */
    width: 100%;
    display:block;
    text-align:center;
    height: 300px;
    color: #FFF;
    transform: translateY(-50%);
    z-index: 1000;
    visibility: hidden;
  }

  .overlay{
    position: fixed;
    width: 100%;
    height: 100%;
    background: rgba(0,0,0,0.7);
    visibility: hidden;
    top: 0;
    left: 0;
    z-index: 1000;
  }

  .loader,
  .loader:before,
  .loader:after {
    border-radius: 50%;
    width: 2.5em;
    height: 2.5em;
    -webkit-animation-fill-mode: both;
    animation-fill-mode: both;
    -webkit-animation: load7 1.8s infinite ease-in-out;
    animation: load7 1.8s infinite ease-in-out;
  }
  .loader {
    color: #ffffff;
    font-size: 10px;
    margin: 80px auto;
    position: relative;
    text-indent: -9999em;
    -webkit-transform: translateZ(0);
    -ms-transform: translateZ(0);
    transform: translateZ(0);
    -webkit-animation-delay: -0.16s;
    animation-delay: -0.16s;
  }
  .loader:before,
  .loader:after {
    content: '';
    position: absolute;
    top: 0;
  }
  .loader:before {
    left: -3.5em;
    -webkit-animation-delay: -0.32s;
    animation-delay: -0.32s;
  }
  .loader:after {
    left: 3.5em;
  }
  @-webkit-keyframes load7 {
    0%,
    80%,
    100% {
      box-shadow: 0 2.5em 0 -1.3em;
    }
    40% {
      box-shadow: 0 2.5em 0 0;
    }
  }
  @keyframes load7 {
    0%,
    80%,
    100% {
      box-shadow: 0 2.5em 0 -1.3em;
    }
    40% {
      box-shadow: 0 2.5em 0 0;
    }
  }

  .show{
    visibility: visible;
  }

  .spinner, .overlay{
    opacity: 0;
    -webkit-transition: all 0.3s;
    -moz-transition: all 0.3s;
    transition: all 0.3s;
  }

  .spinner.show, .overlay.show {
    opacity: 1
  }

  .text-list {
    padding: 0;
  }

  .text-list li {
    border-bottom: 1px solid #ccc;
    list-style-type: none;
    padding: 8px 0;
  }
</style>

<template>
  <div class="container">
  
    <div class="overlay" :class="{ show: !cacheInitialised }"></div>
    <div class="spinner" :class="{ show: !cacheInitialised }">
      <div class="loader"></div>
      <p>Initialising, please wait</p>
    </div>

    <h1>Victorian COVID-19 Public Exposure Sites</h1>
    <div class="row align-items-end">
      <div class="col-md-8">
        <p>The details on this map are intentionally basic - if you see anything within an area that concerns you, please go to <a href="https://www.coronavirus.vic.gov.au/exposure-sites">https://www.coronavirus.vic.gov.au/exposure-sites</a> to view the full details including dates of the COVID-19 exposure sites.</p>
        <p>See below the map for <a href="#ptdetails">Public Transport route details</a>, which have proven tricky to map accurately on the map at this stage.</p>
        <p>If you see any issues with the data, please <a href="mailto:dev@awd.net.au">email me to let me know</a>.</p>
        <br />
        <p>Looking for my radius checker tool? It's at <a href="https://radius-checker.bronthulke.com.au/">Radius Checker</a>, and includes multiple radius mapping!</p>
      </div>
      <div class="col-md-4 legend">
        <h5><strong>Legend:</strong></h5>
        <div class="legend-items">
          <div class="group">
            <div class="legend-item"><img src="https://maps.google.com/mapfiles/ms/icons/red-dot.png" /><span class="text">Tier 1</span></div>
            <div class="legend-item"><img src="https://maps.google.com/mapfiles/ms/icons/blue-dot.png" /><span class="text">Tier 2</span></div>
            <div class="legend-item"><img src="https://maps.google.com/mapfiles/ms/icons/green-dot.png" /><span class="text">Tier 3</span></div>
          </div>
          <button class="btn btn-primary" :disabled="!cacheInitialised" @click="doMapDataLoad">Refresh sites data</button>
        </div>
      </div>
    </div>
     <div class="row map-row">
      <div class="col-12">
        <div id="map"></div>
      </div>
    </div>

    <div class="row">
      <div class="col-12">
        <a name="ptdetails"></a>
        <h5>PUBLIC TRANSPORT</h5>
        <p v-if="ptRecords.Length == 0">
           No current listings
        </p>
        <ul v-else class="text-list">
          <li v-for="item in ptRecords" :key="item.id">
            <strong>{{ item.address }}</strong> {{ formatDateDMY(item) }} {{ item.exposure_time_details }}<br/>
            {{ item.advice_title }}
          </li>
        </ul>

      </div>
    </div>

    <div class="row">
      <div class="col-12">
        <h5>DISCLAIMER</h5>
        <p>The data presented in this website is pulled from the <a href="https://discover.data.vic.gov.au/dataset/all-victorian-sars-cov-2-covid-19-current-exposure-sites" target="_blank">Victorian Government Data API</a>.</p>
        <p>This project is a "passion project" by me, in order to allow me to visually see any nearby COVID-19 exposure sites in Melbourne.</p>
        <p>Although the data is updated on a regular basis, I provide no guarantees about the validity of the data at any point in time. I take no responsibility for any decisions or outcomes that come about as a result of the use of this website.</p>
        <p>Created by Bron Thulke - code available on <a href="https://github.com/bronthulke/vic-exposure-sites">Github</a></p>
      </div>
    </div>
  </div>
</template>
