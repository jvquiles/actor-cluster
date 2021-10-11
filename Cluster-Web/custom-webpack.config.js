const webpack = require('webpack');

console.log("process.env.API_URL: " + process.env.API_URL);

module.exports = {
  plugins: [
    new webpack.DefinePlugin({
      $ENV: {
        api_url: JSON.stringify(process.env.API_URL)
      }
    })
  ]
};