require("dotenv").config(); //import process.env
const express = require("express"); //import express
const connection = require("./config/database");
const webRouters = require("./routes/web");
const cors = require("cors");

const app = express(); //khai bao app
const port = process.env.PORT || 8888; //trong truong hop PORT kh chay thi thuc hien cai con lai
const hostname = process.env.HOST_NAME;

//use cors
app.use(cors());

//config req.body
app.use(express.json()); //for json
app.use(express.urlencoded({ extended: true })); //for form data

//route
app.use("/", webRouters);

//connection
connection;

app.listen(port, hostname, () => {
  console.log(`Example app listening on port ${port}`);
});
