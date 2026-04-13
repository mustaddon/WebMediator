# WebMediator [![NuGet version](https://badge.fury.io/nu/WebMediator.svg?1)](http://badge.fury.io/nu/WebMediator)
A universal WebApi endpoint for any mediators.


## Features
* Suitable for any mediators
* Generics request types support
* In/out file streams support
* Server-sent events support
* Ready-to-use API clients: .NET, JavaScript, Python


## Example 1: WebMediator with MediatR
*.NET CLI*
```
dotnet new web --name "WebMediatorExample"
cd WebMediatorExample
dotnet add package WebMediator
dotnet add package MediatR --version 12.4.1
```

*Change Program.cs*
```C#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PingHandler).Assembly));

var app = builder.Build();

app.MapMediator("mediator", 
    // register possible request types
    cfg => cfg.RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly),
    // invoke the MediatR
    async ctx => await ctx.Services.GetRequiredService<MediatR.IMediator>()
        .Send(await ctx.ReadData(), ctx.CancellationToken)); 

app.Run();
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.MediatR)


## Example 2: Request/Response

*Request*
```
GET /mediator/Ping?data={"message":"TEST"}

or

POST /mediator/Ping
{"message":"TEST"}
```

*Response*
```
{"message":"TEST PONG"}
```

## Example 3: .NET client
```
dotnet add package WebMediator.Client
```

*Program.cs:*
```C#
using WebMediator.Client;


// create client
using var client = new WebMediatorClient("https://localhost:7263/mediator");

// send request
var response = await client.Send(new Ping { Message = "TEST" });

Console.WriteLine(response.Message);
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.Client)


## Example 4: JavaScript client
```
npm i web-mediator-client
```

*JS*
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

let response = await client.send('Ping', { message: 'TEST' });

console.log(response.data);
```

[JS Project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript)


## Example 5: Python client
```
pip install webmediator
```

*code*
```python
import webmediator

client = webmediator.Client('https://localhost:7263/mediator')

response = client.send('Ping', {'message':'EXAMPLE' })
print(response)
```

[Python Project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python)


## Example 6: File upload
*Create RequestHandler*
```C#
public class FileUpload : MediatR.IRequest
{
    public string Name { get; set; } 
    public Stream Content { get; set; } 
}

public class FileUploadHandler : MediatR.IRequestHandler<FileUpload>
{
    public async Task Handle(FileUpload request, CancellationToken cancellationToken)
    {
        var filePath = Path.GetFullPath(request.Name);
        using var fileStream = File.Create(filePath);
        await request.Content.CopyToAsync(fileStream, cancellationToken);
    }
}
```

*Sending a file in JavaScript*
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

let file = document.getElementById('my-input').files[0];

await client.send('FileUpload', { name: file.name, content: file });
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript/test.js)




## Example 7: Generics requests
```C#
app.MapMediator("mediator", 
    // existing generic types will suffice for this example
    cfg => cfg.RegisterTypes([typeof(List<>), typeof(Dictionary<,>)]),
    // for simplicity, return the received data
    ctx => ctx.ReadData()); 
```

*Request #1: Equivalent of List\<String>*
```
POST /mediator/List(String)
["text1","text2","text3"]
```

*Request #2: Equivalent of Dictionary<string,int?[]>*
```
POST /mediator/Dictionary(String-Array(Nullable(Int32)))
{"key1":[555,null,777]}
```



## Example 8: Server-sent events
*Create RequestHandler*
```C#
public class ExampleEventsHandler : IRequestHandler<ExampleAsyncEvents, IAsyncEnumerable<SseItem<string>>>
{
    public async Task<IAsyncEnumerable<SseItem<string>>> Handle(ExampleAsyncEvents request, CancellationToken cancellationToken)
    {
        return ExampleGenerator(request, cancellationToken);
    }

    async IAsyncEnumerable<SseItem<string>> ExampleGenerator(ExampleAsyncEvents request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new($"#{index} example event", request.Type)
            {
                EventId = index.ToString()
            };
            await Task.Delay(1000, cancellationToken);
            index++;
        }
    }
}
```

*JavaScript*
```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

const asyncEvents = client.eventStream('ExampleAsyncEvents', { type: 'test' });

for await (const sse of asyncEvents) { 
    console.log(sse); 
}


//// Console output:
// {type: 'test', data: '#0 example event', lastEventId: '0'}
// {type: 'test', data: '#1 example event', lastEventId: '1'}
// {type: 'test', data: '#2 example event', lastEventId: '2'}
// {type: 'test', data: '#3 example event', lastEventId: '3'}
```


## Example 9: Async streams
*Create RequestHandler*
```C#
public class AsyncItemsStreamHandler : IStreamRequestHandler<AsyncItemsStream, AsyncItem>
{
    public async IAsyncEnumerable<AsyncItem> Handle(AsyncItemsStream request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < request.Count; i++)
        {
            yield return new()
            {
                Index = i,
                Text = $"example async item #{i}"
            };
            await Task.Delay(1000, cancellationToken);
        }
    }
}
```

*adding IStreamRequest handling to the endpoint*
```C#
app.MapMediator("mediator",
    // register possible request types
    cfg => cfg
        .RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly)
        .RegisterTypes([typeof(ExampleAsyncEventsStream)]),

    // handler with IStreamRequest and IRequest calls
    async ctx =>
    {
        var request = await ctx.ReadData();
        var mediatorSvc = ctx.Services.GetRequiredService<MediatR.IMediator>();

        return typeof(MediatR.IBaseRequest).IsAssignableFrom(ctx.DataType)
            ? await mediatorSvc.Send(request, ctx.CancellationToken)
            : mediatorSvc.CreateStream(request, ctx.CancellationToken);
    });
```
[Example project...](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.MediatR)


*JavaScript*
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

