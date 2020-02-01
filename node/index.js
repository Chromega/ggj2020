var static = require('node-static');
var http = require('http');
const PORT = process.env.PORT || 5000

var file = new(static.Server)();

http.createServer(function (req, res) {
  file.serve(req, res);
}).listen(PORT);
