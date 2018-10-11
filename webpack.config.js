var path = require("path");
const resolve = p => path.resolve(__dirname, p);

var babelOptions = {
  presets: [
    ["@babel/preset-env", {
        "modules": false,
        "useBuiltIns": "usage",
    }]
  ]
};

module.exports = function (evn, argv) {
  var mode = argv.mode || "development";
  var isProduction = mode === "production";
  console.log("Webpack mode: " + mode);
 
  return {
    mode: mode,
    devtool: isProduction ? false : "eval-source-map",
    entry: {
      app: ["@babel/polyfill", './Fable.Parsimmon.Tests/Fable.Parsimmon.Tests.fsproj']
    },
    output: {
      filename: 'bundle.js',
      path: path.join(__dirname, './public'),
    },
    resolve: {
     modules: [ "node_modules/", resolve("./node_modules/") ]
    },
    devServer: {
      contentBase: './public',
      port: 8080
    },
    module: {
      rules: [
        {
          test: /\.fs(x|proj)?$/,
          use: "fable-loader"
        },
        {
          test: /\.js$/,
          exclude: /node_modules/,
          use: {
            loader: 'babel-loader',
            options: babelOptions
          },
        }
      ]
    }
  };
}