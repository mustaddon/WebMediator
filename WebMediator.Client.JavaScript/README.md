# web-mediator-client [![npm version](https://badge.fury.io/js/web-mediator-client.svg)](https://www.npmjs.com/package/web-mediator-client)
WebMediator API client


```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

let res = await client.send('Ping', { Message: 'TEXT' });

console.log(res);
```
