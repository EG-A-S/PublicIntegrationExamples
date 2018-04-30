# Lindbak Coupon-service example
This is an example of how to use the Lindbak Coupon-service API in Node.js.

## Getting started
First you have to insert values for all the undefined variables in index.js:
* url_to_oath_server
* my_client_id
* my_client_secret
* my_resource
* url_to_couponservice

then run:
```sh
npm install
npm start
```

If the server started without errors, then...
```
localhost:3000/coupons?memberNumber=1
```
should return all the coupons belonging to the member with memberNumber=1

## PS:
This example-server does not authenticate clients, this is just an example of how to use the Lindbak Coupon-service API.
