var getRawBody = require('raw-body');
var getFormBody = require('body/form');
var body = require('body');
var axios = require('axios');
var fs = require('fs');


/*
To enable the initializer feature (https://help.aliyun.com/document_detail/156876.html)
please implement the initializer function as below：
exports.initializer = (context, callback) => {
  console.log('initializing');
  callback(null, '');
};
*/

exports.handler = (req, resp, context) => {
    var api_token = fs.readFileSync('api-token', 'utf8').trim();

    var params = {
        path: req.path,
        queries: req.queries,
        headers: req.headers,
        method : req.method,
        requestURI : req.url,
        clientIP : req.clientIP,
    }
        
    getRawBody(req, async function(err, body) {
        for (var key in req.queries) {
          var value = req.queries[key];
          resp.setHeader(key, value);
        }
        try {
            const strBody = body;
            body = JSON.parse(body);
            console.log(body);
            const owner = "jsc723";
            const repo = "misaka-patches-headers";
            const fileName = `${body['game']}|${body['author']}|${body['md5']}`
            const path = `headers/${fileName}`;
            const content = Buffer.from(strBody, 'utf-8').toString('base64');
        
            const result = await axios.post(`https://gitee.com/api/v5/repos/${owner}/${repo}/contents/${path}`, {
                access_token: api_token,
                content: content,
                message: 'create patch for' + body['game']
            });
            console.log(result.data);
            resp.send("success");
        } catch (error) {
            if (error.response) {
            // The request was made and the server responded with a status code
            // that falls out of the range of 2xx
            console.log(error.response.data);
            console.log(error.response.status);
            console.log(error.response.headers);
            resp.send("error: " + error.response.data.message);
          } else if (error.request) {
            // The request was made but no response was received
            // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
            // http.ClientRequest in node.js
            console.log(error.request);
            resp.send("error: " + error.request);
          } else {
            // Something happened in setting up the request that triggered an Error
            console.log('Error', error.message);
            resp.send("error: " + error.message);
          }
        }
    }); 
      
    /*
    getFormBody(req, function(err, formBody) {
        for (var key in req.queries) {
          var value = req.queries[key];
          resp.setHeader(key, value);
        }
        params.body = formBody;
        console.log(formBody);
        resp.send(JSON.stringify(params));
    }); 
    */
}