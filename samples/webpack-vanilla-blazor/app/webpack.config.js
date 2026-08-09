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
        {
          // favicon.png is a .NET static web asset resolved by unplugin-dotnet-wasm.
          // Emit it at its plain name so the <link rel="icon"> in index.html resolves.
          test: /\.png$/,
          type: 'asset/resource',
          generator: { filename: '[name][ext]' },
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
        logLevel: 'info',
      }),
      new MiniCssExtractPlugin({
        filename: 'assets/[name].css',
      }),
      new HtmlWebpackPlugin({
        template: './index.html',
        // If you load the entry as a module then Blazor's boot script does NOT auto-start
        // scriptLoading: 'module',
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
