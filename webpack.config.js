const Path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const Webpack = require("webpack");

const commonPlugins = [
    new HtmlWebpackPlugin({
        filename: "index.html",
        template: "./src/Botto.Options/options.html",
    }),
]

const resolve = path => Path.isAbsolute(path) ? path : Path.join(__dirname, path);

const makeConfig = isProduction => {
    console.log(`${isProduction ? "Production" : "Development"} build...`);
    return {
        mode: isProduction ? "production" : "development",
        devtool: isProduction ? false : "eval-source-map",
        entry: isProduction
            ? {
                content: [
                    resolve("./src/Botto.Content/Botto.Content.fsproj"),
                ],
                options: [
                    resolve("./src/Botto.Options/Botto.Options.fsproj"),
                    resolve("./src/Botto.Options/options.scss"),
                ],
            }
            : {
                content: [
                    resolve("./src/Botto.Content/Botto.Content.fsproj"),
                ],
                options: [
                    resolve("./src/Botto.Options/Botto.Options.fsproj"),
                ],
                style: [
                    resolve("./src/Botto.Options/options.scss"),
                ],
            },
        output: {
            filename: "[name].js",
            path: resolve("dist"),
        },
        module: {
            rules: [
                {
                    test: /\.fs(x|proj)?$/,
                    use: "fable-loader",
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: {},
                    },
                },
                {
                    test: /\.(sass|scss|css)$/,
                    use: [
                        isProduction ? MiniCssExtractPlugin.loader : 'style-loader',
                        'css-loader',
                        'sass-loader',
                    ],
                },
            ]
        },
        devServer: {
            port: 8080,
            proxy: {
                "/socket/*": {
                    target: "http://localhost:8085",
                    ws: true
                }
            },
            hot: true,
            open: true,
            inline: true,
            contentBase: resolve("public"),
        },
        optimization: {
            minimizer: isProduction
                ? [new TerserPlugin({ terserOptions: { compress: { drop_console: true } } })]
                : [],
            runtimeChunk: "single",

        },
        resolve: {
            symlinks: false,
        },
        plugins: isProduction
            ? commonPlugins.concat([
                new CopyWebpackPlugin({ patterns: [{ from: "public" }] }),
                new MiniCssExtractPlugin({ filename: "style.css" }),

            ])
            : commonPlugins.concat([
                new Webpack.HotModuleReplacementPlugin(),
            ]),
    };
};

module.exports = env => makeConfig(process.env.NODE_ENV == "production");