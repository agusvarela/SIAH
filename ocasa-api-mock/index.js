var express = require('express');
var app = express();

app.get('/reclamo', (req, res) => {
  const { name } = req.query;
  if (name == 'success') {
    res.status(200).send('TODO GOOD');
  } else {
    res.status(500).send('TODO MAL Bro');
  }
});

app.listen(3000, () => {
  console.log('Example app listening on port 3000!');
});
