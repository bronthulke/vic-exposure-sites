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


### Lints and fixes files

```bash
npm run lint
```

### Customize configuration

See [Configuration Reference](https://cli.vuejs.org/config/).
