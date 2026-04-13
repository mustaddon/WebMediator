import { WebMediatorClient } from './index.js';

const client = new WebMediatorClient('https://localhost:7263/mediator');


const res0 = await client.send('Echo');
console.log(res0);

const res1 = await client.send('Ping', { Message: 'TEXT' });
console.log(res1);

await client.send('FileUpload', { Name: 'test.txt', Content: new Blob(['text text text'], { type: 'text/plain' }) });

const res3 = await client.send('FileDownload', { Name: 'test.txt' });
console.log(res3, await res3.data.text());

const res4 = await client.send('FileDownloadWithInfo', { Name: 'test.txt' });
console.log(res4, await res4.data.content.text());



const res5 = await client.send('ExampleAsyncEvents');
console.log(res5);

let i=0;
for await (const item of res5.data) { 
    console.log(item); 
    if(i++ > 2) break; 
}



for await (const sse of client.eventStream('ExampleAsyncEvents')) { 
    console.log(sse); 
}



// for await (const sse of client.eventStream('AsyncEventsSse', { type: 'test', ErrorIndex: 3 }))
//     console.log(sse); 