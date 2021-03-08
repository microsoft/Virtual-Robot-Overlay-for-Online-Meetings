/**
 *
 * server.js
 * Node.js Server
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


/* MAIN SERVER (Static) */

const node_static = require('node-static');
const staticServer = new node_static.Server('../VROOM-360Broadcaster');

const mainServer = https.createServer(options, function(request, response)
{
	request.addListener('end', function() {
		staticServer.serve(request, response);
	}).resume();
});

mainServer.listen(8082);


/* DSS SERVER */

const finalhandler = require('finalhandler');
const debug = require('debug')('dss:boot');
const router = require('./node-dss');

const dssServer = http.createServer(function (req, res)
{
  router(req, res, finalhandler(req, res));
});

const bind = dssServer.listen(process.env.PORT || 3000, () =>
{
  debug(`online @ ${bind.address().port}`);
});


/* STATUS */

var serverStartedDate = Date.now();
console.log('Server started. [' + (new Date()).toString() + ']');
