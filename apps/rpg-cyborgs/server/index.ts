import * as path from 'path'
import express from 'express'
import type { Request, Response } from 'express'

import { createProxyMiddleware } from 'http-proxy-middleware'
//import type { Filter, Options, RequestHandler } from 'http-proxy-middleware'
import dotenv from 'dotenv'
dotenv.config()

const app = express()
const port = process.env.PORT || 3000
const DIST_DIR = path.join(__dirname, '../dist') // NEW
const HTML_FILE = path.join(DIST_DIR, 'index.html') // NEW

const proxyMiddleware = createProxyMiddleware<Request, Response>({
  target: `${process.env.RPG_API_HOST}api/rpg/`,
  changeOrigin: true,
  secure: false,
  logger: console,
})

console.log('Server Target', process.env.RPG_API_HOST)
app.use(express.static(DIST_DIR)) // NEW

app.use('/api', proxyMiddleware)

// app.get('/api', (req, res) => {
//   res.send(mockResponse)
// })
app.get('/', (req, res) => {
  console.log('Request', req.path)
  res.sendFile(HTML_FILE) // EDIT
})
app.listen(port, function () {
  console.log('App listening on port: ' + port)
})
