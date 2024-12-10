import * as path from 'path'
import * as express from 'express'
import type { Request, Response, NextFunction } from 'express'

import { createProxyMiddleware } from 'http-proxy-middleware'
import type { Filter, Options, RequestHandler } from 'http-proxy-middleware'

const app = express()
const port = process.env.PORT || 3000
const DIST_DIR = path.join(__dirname, '../dist') // NEW
const HTML_FILE = path.join(DIST_DIR, 'index.html') // NEW

const proxyMiddleware = createProxyMiddleware<Request, Response>({
  target: `${process.env.RPG_API_URL}api/rpg/`,
  changeOrigin: true,
})

app.use(express.static(DIST_DIR)) // NEW

app.use('/api', proxyMiddleware)

// app.get('/api', (req, res) => {
//   res.send(mockResponse)
// })
app.get('/', (req, res) => {
  res.sendFile(HTML_FILE) // EDIT
})
app.listen(port, function () {
  console.log('App listening on port: ' + port)
})
