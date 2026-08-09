const path = require('node:path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const DotnetWasm = require('unplugin-dotnet-wasm/webpack').default;

module.exports = (env, argv) => {
  const isProduction = argv.mode === 'production';

  return {
    entry: './src/index.ts',
    target: 'web',
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'assets/[name].js',
      assetModuleFilename: 'assets/[name]-[contenthash][ext]',
      publicPath: '/',
      clean: true,
    },
    resolve: {
      extensions: ['.ts', '.js'],
    },
    module: {
      rules: [
        {
          test: /\.ts$/,
          use: {
            loader: 'ts-loader',
            options: { transpileOnly: true },
          },
          exclude: /node_modules/,
        },
        {
          test: /\.css$/,
          use: [MiniCssExtractPlugin.loader, 'css-loader'],
        },
        {
          // The Weather page fetches this JSON at runtime; emit it at a stable path
          // so Blazor's `HttpClient` GET of `sample-data/weather.json` resolves.
          test: /sample-data[\\/].+\.json$/,
          type: 'asset/resource',
          generator: { filename: 'sample-data/[name][ext]' },
        },
      ],
    },
    plugins: [
      // Makes the fast `dotnet build` output of the Blazor Library directly bundleable,
      // and serves the runtime + static web assets through the dev server.
      DotnetWasm({
        projectName: 'Library',
        projectRoot: path.resolve(__dirname, '../Library'),
        configuration: isProduction ? 'Release' : 'Debug',
        targetFramework: 'net10.0',
        isPublish: isProduction,
        logLevel: 'debug',
      }),
      new MiniCssExtractPlugin({
        filename: 'assets/[name].css',
      }),
      new HtmlWebpackPlugin({
        template: './index.html',
        // Load the entry as an ES module so Blazor's boot script (bundled into it)
        // sees `document.currentScript === null` and does NOT auto-start; we start it
        // ourselves in src/index.ts so we can pass boot options.
        scriptLoading: 'module',
      }),
    ],
    devServer: {
      // Blazor's client-side router needs unknown routes to fall back to index.html.
      historyApiFallback: true,
      hot: false,
      open: true,
      port: 5080,
    },
    // Source maps that don't choke on the dotnet runtime's emitted JS.
    devtool: isProduction ? false : 'source-map',
    // The .NET SDK's `blazor.webassembly.js` uses dynamic requires that webpack
    // can't statically analyze; the plugin still resolves them correctly at runtime.
    ignoreWarnings: [
      { message: /Critical dependency: the request of a dependency is an expression/ },
    ],
  };
};
