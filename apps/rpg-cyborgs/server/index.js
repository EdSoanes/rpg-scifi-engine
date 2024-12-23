"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
var path = __importStar(require("path"));
var express_1 = __importDefault(require("express"));
var http_proxy_middleware_1 = require("http-proxy-middleware");
//import type { Filter, Options, RequestHandler } from 'http-proxy-middleware'
var dotenv_1 = __importDefault(require("dotenv"));
dotenv_1.default.config();
var app = (0, express_1.default)();
var port = process.env.PORT || 3000;
var DIST_DIR = path.join(__dirname, '../dist'); // NEW
var HTML_FILE = path.join(DIST_DIR, 'index.html'); // NEW
var proxyMiddleware = (0, http_proxy_middleware_1.createProxyMiddleware)({
    target: "".concat(process.env.RPG_API_HOST, "api/rpg/"),
    changeOrigin: true,
    secure: false,
    logger: console,
});
console.log('Server Target', process.env.RPG_API_HOST);
app.use(express_1.default.static(DIST_DIR)); // NEW
app.use('/api', proxyMiddleware);
// app.get('/api', (req, res) => {
//   res.send(mockResponse)
// })
app.get('/', function (req, res) {
    console.log('Request', req.path);
    res.sendFile(HTML_FILE); // EDIT
});
app.listen(port, function () {
    console.log('App listening on port: ' + port);
});
