const express = require('express');
const app = express();

app.use(express.json());

app.get('/reclamo', (req, res) => {
  const { name } = req.query;
  if (name == 'success') {
    res.status(200).send('Accepted');
  } else {
    res.status(500).send('Error');
  }
});

app.post('/envio', (req, res) => {
  const pedidos = req.body;
  const trackingNumbers = pedidos.map(element => {
    return { idPedido: element.id, tracking: generateTrackingNumber() };
  });
  console.log(trackingNumbers);
  res.status(200).send(trackingNumbers);
});

const generateTrackingNumber = () => {
  let trackingNumber = 'EC';
  trackingNumber += Math.round(Math.random() * 100000000000);
  return trackingNumber;
};

app.listen(3000, () => {
  console.log('OCASA app listening on port 3000!');
});
