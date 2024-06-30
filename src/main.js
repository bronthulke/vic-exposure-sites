import Vue from 'vue'
import App from './App.vue'

import 'bootstrap/dist/css/bootstrap.css'
import '@/styles.css'

Vue.config.productionTip = false

const appInsights = new ApplicationInsights({ 
  config: {
    connectionString: "process.env.VUE_APP_APPINSIGHTS_CONNECTION_KEY"
    /* …Other Configuration Options… */
  }
});
  
appInsights.loadAppInsights();

appInsights.trackPageView();
  
new Vue({
  render: h => h(App),
}).$mount('#app')
