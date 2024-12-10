"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var path = require("path");
var express = require("express");
var http_proxy_middleware_1 = require("http-proxy-middleware");
var app = express();
var port = process.env.PORT || 3000;
var DIST_DIR = path.join(__dirname, '../dist'); // NEW
var HTML_FILE = path.join(DIST_DIR, 'index.html'); // NEW
var proxyMiddleware = (0, http_proxy_middleware_1.createProxyMiddleware)({
    target: "".concat(process.env.RPG_API_URL, "api/rpg/"),
    changeOrigin: true,
});
app.use(express.static(DIST_DIR)); // NEW
app.use('/api', proxyMiddleware);
// app.get('/api', (req, res) => {
//   res.send(mockResponse)
// })
app.get('/', function (req, res) {
    res.sendFile(HTML_FILE); // EDIT
});
app.listen(port, function () {
    console.log('App listening on port: ' + port);
});
