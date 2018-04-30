const express = require('express');
const request = require('request');

const app = express();
const port = process.env.PORT || 3000;

app.get('/coupons', (req, res) => getToken(req, res, getCoupons));
app.listen(port, () => console.log("Server running at http://localhost:%d", port));

/* Sends a post-request to get an access token, then calls the callback with
that token */
function getToken(req, res, callback) {
    const options = {
        'method': 'POST',
        'url': url_to_oauth_server,
        'headers': {
            'cache-control': 'no-cache',
            'content-type': 'application/x-www-form-urlencoded'
        },
        'form': {
            'client_id': my_client_id,
            'client_secret': my_client_secret,
            'grant_type': 'client_credentials',
            'resource': my_resource
        }
    };

    request(options, (error, response, body) => {
        if (error) throw new Error(error);
        token = JSON.parse(body).access_token;
        callback(req, res, token);
    });
}

/* GETS all coupons for the specied memberNumber */
function getCoupons(req, res, token) {
    const options = {
        'method': 'GET',
        'url': url_to_couponservice,
        'qs': {'memberNumber': req.query.memberNumber},
        'headers': {
            'cache-control': 'no-cache',
            'authorization': 'Bearer ' + token
        }
    };

    request(options, (error, response, body) => {
        if (error) throw new Error(error);
        res.send(body);
    });
}
