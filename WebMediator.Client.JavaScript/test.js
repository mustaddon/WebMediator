import { WebMediatorClient } from './index.js';

const client = new WebMediatorClient('https://localhost:7263/mediator');


let res0 = await client.send('Echo');
console.log(res0);

let res1 = await client.send('Ping', { Message: 'TEXT' });
console.log(res1);

await client.send('FileUpload', { Name: 'test.txt', Content: new Blob(['text text text'], { type: 'text/plain' }) });

let res3 = await client.send('FileDownload', { Name: 'test.txt' });
console.log(res3, await res3.data.text());

let res4 = await client.send('FileDownloadWithInfo', { Name: 'test.txt' });
console.log(res4, await res4.data.Content.text());