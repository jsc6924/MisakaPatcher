const axios = require('axios');
var fs = require('fs');
async function f1() {
    const owner = "jsc723";
    const repo = "misaka-patches-headers";
    const path = "headers/test3";
    const plain = "你好";
    const api_token = fs.readFileSync('api-token', 'utf8').trim();
    const buffer = Buffer.from(plain, 'utf-8').toString('base64');

    try {
        var result = await axios.post(`https://gitee.com/api/v5/repos/${owner}/${repo}/contents/${path}`, {
            access_token: api_token,
            content: buffer,
            message: 'test'
        });
        console.log(result.status);  
        console.log(result.data);
    } catch (error) {
        if (error.response) {
            // The request was made and the server responded with a status code
            // that falls out of the range of 2xx
            console.log(error.response.data);
            console.log(error.response.status);
            console.log(error.response.headers);
          } else if (error.request) {
            // The request was made but no response was received
            // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
            // http.ClientRequest in node.js
            console.log(error.request);
          } else {
            // Something happened in setting up the request that triggered an Error
            console.log('Error', error.message);
          }
          console.log(error.config);
    }
    return result;
}

var a = f1();
