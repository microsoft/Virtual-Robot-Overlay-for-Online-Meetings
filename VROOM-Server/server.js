/**
 *
 * server.js
 * VROOM Server app (Node.js)
 *
 */


"use strict";

const fs = require('fs');

const options = {
  key: fs.readFileSync('key.pem'),
  cert: fs.readFileSync('cert.pem')
};

const http = require('http');
const https = require('https');


/* Web/static server */

const node_static = require('node-static');
const staticServer = new node_static.Server('../VROOM-360Broadcaster');

const webServer = https.createServer(options, function(request, response)
{
	request.addListener('end', function() {
		staticServer.serve(request, response);
	}).resume();
});

webServer.listen(8082);


/* Server for events and WebRTC signaling */

const finalhandler = require('finalhandler');
const debug = require('debug')('dss:boot');
const router = require('./dss-and-events');

// Non-secure DSS/event server (for Unity), on port 3000
const dssServer = http.createServer(function (req, res)
{
  router(req, res, finalhandler(req, res));
});
const bind = dssServer.listen(process.env.PORT || 3000, () =>
{
  debug(`online @ ${bind.address().port}`);
});

// Secure DSS/event server (for web browsers), on port 3001
const dssServerSecure = https.createServer(options, function (req, res)
{
  router(req, res, finalhandler(req, res));
});
const bindSec = dssServerSecure.listen(process.env.PORT || 3001, () =>
{
  debug(`online @ ${bindSec.address().port}`);
});


/* Status */

var serverStartedDate = Date.now();
console.log('Server started. [' + (new Date()).toString() + ']');
