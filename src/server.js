import express from "express";
import initWebRoutes from "./routes/web";
import bodyParser from "body-parser";
import connecttion from "./config/connectDB";

require("dotenv").config();

const app = express();
const PORT = process.env.PORT || 8080;

//config body-parser
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

//test connection db
connecttion();

//init web routes
initWebRoutes(app);

app.listen(PORT, () => {
  console.log(">>> Backend is running on the port = " + PORT);
});