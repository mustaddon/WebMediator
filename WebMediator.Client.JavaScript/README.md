# web-mediator-client [![npm version](https://badge.fury.io/js/web-mediator-client.svg?3)](https://www.npmjs.com/package/web-mediator-client)
JavaScript client for the WebMediator API.

```
npm i web-mediator-client
```

*JS:*
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

let response = await client.send('Ping', { Message: 'TEST' });

console.log(response.data);
```

*Console output:*
```
{"Message":"TEST PONG"}
```