# web-mediator-client [![npm version](https://badge.fury.io/js/web-mediator-client.svg?3)](https://www.npmjs.com/package/web-mediator-client)
JavaScript client for the WebMediator API.

```
npm i web-mediator-client
```

## Example 1: Request/response
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

const response = await client.send('Ping', { message: 'TEST' });

console.log(response.data);


//// Console output:
// {"message":"TEST PONG"}
```


## Example 2: File upload/download
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');


const fileData = new Blob(["Hello, world!"], { type: "text/plain" });
// OR 
// fileData = document.getElementById('my-input').files[0];

await client.send('FileUpload', { name: 'example.txt', content: fileData });

let response = await client.send('FileDownload', { name: 'example.txt' });
console.log(response, await response.data.text());
```


## Example 3: Async streams
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

const response = await client.send('AsyncItemsStream', { count: 3 });

for await (const item of response.data) { 
    console.log(item); 
}

//// Console output:
// {index: 0, text: 'example async item #0'}
// {index: 1, text: 'example async item #1'}
// {index: 2, text: 'example async item #2'}
```


## Example 4: Server-sent events
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

const asyncEvents = client.eventStream('ExampleAsyncEvents');

for await (const sse of asyncEvents) { 
    console.log(sse); 
}


//// Console output:
// {type: 'test', data: '#0 example event', lastEventId: '0'}
// {type: 'test', data: '#1 example event', lastEventId: '1'}
// {type: 'test', data: '#2 example event', lastEventId: '2'}
// {type: 'test', data: '#3 example event', lastEventId: '3'}
```