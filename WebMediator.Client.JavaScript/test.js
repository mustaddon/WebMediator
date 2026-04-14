import { WebMediatorClient } from './index.js';

const client = new WebMediatorClient('https://localhost:7263/mediator');


console.log(await client.send('Echo'));


const res1 = await client.send('Ping', { Message: 'TEXT' });
// OR
// res1 = await client.send({ type: 'Ping', data: { Message: 'TEXT' } });
console.log(res1);


await client.send('FileUpload', { Name: 'test.txt', Content: new Blob(['text text text'], { type: 'text/plain' }) });


const res3 = await client.send('FileDownload', { Name: 'test.txt' });
console.log(res3, await res3.data.text());


const res4 = await client.send('FileDownloadWithInfo', { Name: 'test.txt' });
console.log(res4, await res4.data.content.text());


const res5 = await client.send('AsyncItemsStream', { count: 3, delay: 1000 });
for await (const item of res5.data) { 
    console.log(item); 
}


for await (const sse of client.eventStream('ExampleAsyncEvents', { type: 'test' }, { 
    // reconnectionDelay: 1000,
    // reconnectionRetriesLimit: 3,
    // signal: AbortSignal.timeout(4000)
})) { 
    console.log(sse); 
}



// for await (const sse of client.eventStream('AsyncEventsSse', { type: 'test', ErrorIndex: 3 }))
//     console.log(sse); 