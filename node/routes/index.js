var express = require("express");
var router = express.Router();
/* GET home page. */
var fs = require("fs");
router.get("/", function (req, res, next) {
  db_data = [{ name: "Error user", age: 25 }];

  // make a post request to the server https://adb-7241440174863553.13.azuredatabricks.net/api/2.0/sql/statements with bearer token and payload
  // get the response and pass it to the index.pug file

  payload = JSON.stringify({
    warehouse_id: "791486f2d3293fcd",
    statement:
      "select * from `ida-sandbox-unitycatalog`.`enriched-unharmonized-techx01`.employee",
    wait_timeout: "50s",
  });

  fetch(
    "https://adb-7241440174863553.13.azuredatabricks.net/api/2.0/sql/statements",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + process.env.DB_TOKEN,
      },
      body: payload,
    }
  )
    .then((response) => response.json())
    .then((data) => {
      db_data = data["result"]["data_array"];
      res.render("index", { title: "Express", data: db_data });
    })
    .catch((error) => {
      console.error("Error:", error);

      fs.readFile("fallback.csv", "utf8", (err, data) => {
        db_data = data.split("\n").map((row) => {

          columns = row.split(",");
          console.log(columns);
          return [columns[0],columns[1],columns[2],columns[3],columns[4],columns[5],columns[6],columns[7]];
        });

        console.log(db_data);
        res.render("index", { title: "Express", data: db_data });
      });

    });
});

module.exports = router;
