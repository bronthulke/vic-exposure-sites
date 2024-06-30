const Dotenv = require('dotenv-webpack');

module.exports = {
    configureWebpack: {
    //   devtool: "source-map",
      plugins: [
        new Dotenv()
      ]
    },
    devServer: {
      proxy: {
        "/api": {
          target: "http://localhost:7071",
          ws: true,
          changeOrigin: true,
        },
      },
    },
  };