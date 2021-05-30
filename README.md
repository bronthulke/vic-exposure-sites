# Victoria COVID-19 Public Exposure Sites Map

I wanted to see a blue duck, so I drew a blue duck.

(I wanted to see the sites near me, without trolling through a long list and looking for suburb names... so I built a map!)

This site is an [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/overview) site using [Vue.js](https://vuejs.org/).  It uses Azure Functions to communicate with a CosmosDB for caching geocode data.

If you want to fork and run it yourself, here are helpers:

## Project setup

```bash
npm install
```

### Compiles and hot-reloads for development

```bash
npm run serve
```

### Compiles and minifies for production

```bash
npm run build
```

### Compiles and minifies for production, and runs it locally using the Static Web Apps CLI

```bash
npm run servelocal
```

For more info about using this, see [https://docs.microsoft.com/en-us/azure/static-web-apps/add-api?tabs=vue](https://docs.microsoft.com/en-us/azure/static-web-apps/add-api?tabs=vue).

**Local debugging**

I found that when runnign with the Static Web Apps CLI, it was not giving me the abiltity to properly debug my code.  So the workaround I used was to run the Azure Functions locally using the following command:

```base
swa start . --api api 
```

and then running the Vue app using this command:

```base
npm run serve
```

And just ensure that the API URLs are pointing at the site fired up by `swa` (e.g. `http://192.168.1.178:4280/api/...`)

### Lints and fixes files

```bash
npm run lint
```

### Customize configuration

See [Configuration Reference](https://cli.vuejs.org/config/).

## Coming Soon
- removing old/expired sites drop off the map (currently it just adds any new sites, it never deletes ones that have been removed)
- move to server-side Google API for geocoding (important for initial data load, less important now the base set has been loaded)