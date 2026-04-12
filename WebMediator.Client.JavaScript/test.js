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



let res5 = await client.send('ExampleAsyncEvents');
console.log(res5);

let i=0;
for await (const item of res5.data) { 
    console.log(item); 
    if(i++ > 2) break; 
}



let asyncEvents = client.eventStream('ExampleAsyncEvents');
console.log(asyncEvents);

i=0;
for await (const item of asyncEvents) { 
    console.log(item); 
    if(i++ > 5) break; 
}