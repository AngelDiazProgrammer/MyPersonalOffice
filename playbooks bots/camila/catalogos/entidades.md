---
bot: camila
catalog: entities
---
## Entidades

Formato que debe tener un dato que el bot pide.

### Entidad · Numero_Documento (KIND_REGEXP)

Valores válidos: `\d{5,15}`

```json camila:entity
{"name":"Numero_Documento","kind":"KIND_REGEXP","values":["\\d{5,15}"]}
```

### Entidad · placa-moto (KIND_REGEXP)

Valores válidos: `[A-Z]{3}[0-9]{2}[A-I]{1}`

```json camila:entity
{"name":"placa-moto","kind":"KIND_REGEXP","values":["[A-Z]{3}[0-9]{2}[A-I]{1}"]}
```

### Entidad · placa_carro (KIND_REGEXP)

Valores válidos: `[A-Z]{3}[0-9]{3}`

```json camila:entity
{"name":"placa_carro","kind":"KIND_REGEXP","values":["[A-Z]{3}[0-9]{3}"]}
```

### Entidad · tipoDespedida (KIND_MAP)

Valores válidos: 

```json camila:entity
{"name":"tipoDespedida","kind":"KIND_MAP","values":[]}
```

