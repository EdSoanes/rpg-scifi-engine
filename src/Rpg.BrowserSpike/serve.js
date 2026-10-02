// A static file server for the published spike. usage: node serve.js <port> [folder]
// Serves the brotli-compressed file when the browser accepts it, as a real host would.
const http = require('http');
const fs = require('fs');
const path = require('path');

const port = parseInt(process.argv[2] || '5020', 10);
const root = path.resolve(process.argv[3] || path.join(__dirname, 'bin/publish/wwwroot'));
const types = {
  '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css',
  '.wasm': 'application/wasm', '.json': 'application/json', '.png': 'image/png',
  '.dat': 'application/octet-stream', '.dll': 'application/octet-stream', '.pdb': 'application/octet-stream'
};

http.createServer((req, res) => {
  let rel = decodeURIComponent(req.url.split('?')[0]);
  if (rel === '/') rel = '/index.html';

  let file = path.join(root, rel);
  if (!file.startsWith(root)) { res.writeHead(403); return res.end(); }
  if (!fs.existsSync(file) || fs.statSync(file).isDirectory()) file = path.join(root, 'index.html');

  const headers = { 'Content-Type': types[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-cache' };
  if ((req.headers['accept-encoding'] || '').includes('br') && fs.existsSync(file + '.br')) {
    headers['Content-Encoding'] = 'br';
    file += '.br';
  }

  res.writeHead(200, headers);
  fs.createReadStream(file).pipe(res);
}).listen(port, () => console.log(`Serving ${root} on http://localhost:${port}`));
