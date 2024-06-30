import Vue from 'vue'
import App from './App.vue'
import { ApplicationInsights } from '@microsoft/applicationinsights-web'

import 'bootstrap/dist/css/bootstrap.css'
import '@/styles.css'

Vue.config.productionTip = false

const appInsights = new ApplicationInsights({ 
  config: {
    connectionString: "InstrumentationKey=af9a5c48-6afb-4b37-ae5b-bec83723a2dd;IngestionEndpoint=https://westus2-0.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/;ApplicationId=0bbe6ded-07fa-497a-b09a-90c2eda99659"
    /* …Other Configuration Options… */
  }
});
  
appInsights.loadAppInsights();

appInsights.trackPageView();
  
new Vue({
  render: h => h(App),
}).$mount('#app')
