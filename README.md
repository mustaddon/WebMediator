# WebMediator [![NuGet version](https://badge.fury.io/nu/WebMediator.svg?1)](https://www.nuget.org/packages/WebMediator)

Expose mediator requests through a lightweight ASP.NET Core endpoint. WebMediator resolves a request type from the URL, deserializes the request, and passes it to the mediator delegate you provide. It is not tied to a specific mediator library.

## Features

- Integrates with MediatR, Mediator, or a custom dispatcher
- Supports request/response and generic request types
- Handles file and stream input/output
- Supports server-sent events (SSE) and asynchronous streams
- Includes ready-to-use .NET, JavaScript, and Python clients

## Contents

- [Getting started](#getting-started)
- [Configure an endpoint with MediatR](#example-1-configure-an-endpoint-with-mediatr)
- [Request and response](#example-2-request-and-response)
- [Clients (.NET, JavaScript, Python)](#examples-3-5-clients)
- [File upload](#example-6-file-upload)
- [Generic requests](#example-7-generic-requests)
- [Server-sent events](#example-8-server-sent-events)
- [Async streams](#example-9-async-streams)
- [License](#license)

## Getting started

The server package currently targets **.NET 10**. Add it and a mediator package to an ASP.NET Core app. The example below uses MediatR:

```bash
dotnet new web --framework net10.0 --name WebMediatorExample
cd WebMediatorExample
dotnet add package WebMediator
dotnet add package MediatR --version 12.4.1
```

## Example 1: Configure an endpoint with MediatR

Register the request types that clients are allowed to address, then connect WebMediator to MediatR:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PingHandler).Assembly));

var app = builder.Build();

app.MapMediator("mediator",
    // Expose request types from this assembly.
    cfg => cfg.RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly),
    // Deserialize the request and dispatch it through MediatR.
    async ctx => await ctx.Services.GetRequiredService<MediatR.IMediator>()
        .Send(await ctx.ReadData(), ctx.CancellationToken));

app.Run();
```

The endpoint is available at `/mediator/{request-type}`. Only register request types you intend to expose; apply your own authentication, authorization, and validation policies as appropriate.

See the [complete MediatR example](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.MediatR).


## Example 2: Request and response

Requests can be sent as JSON in the POST body, or as the URL-encoded `data` query parameter on a GET request:

```http
POST /mediator/Ping
Content-Type: application/json

{"message":"TEST"}
```

Equivalent GET request:

```http
GET /mediator/Ping?data=%7B%22message%22%3A%22TEST%22%7D
```

The mediator handles the request and returns its result as JSON:

```json
{"message":"TEST PONG"}
```

## Examples 3-5: Clients

Use one of the ready-to-use clients to send requests to the mapped endpoint.

### .NET client

Install the client package:

```bash
dotnet add package WebMediator.Client
```

```csharp
using WebMediator.Client;

using var client = new WebMediatorClient("https://localhost:7263/mediator");
var response = await client.Send(new Ping { Message = "TEST" });

Console.WriteLine(response.Message);
```

This example uses the `Ping` request from the [sample project](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.Client).

### JavaScript client

```bash
npm install web-mediator-client
```

```js
import { WebMediatorClient } from 'web-mediator-client';

const client = new WebMediatorClient('https://localhost:7263/mediator');
const response = await client.send('Ping', { message: 'TEST' });

console.log(response.data);
```

See the [JavaScript client project](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript).

### Python client

```bash
pip install webmediator
```

```python
import webmediator

client = webmediator.Client('https://localhost:7263/mediator')
response = client.send('Ping', {'message': 'TEST'})

print(response)
```

See the [Python client project](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.Python).


## Example 6: File upload

A request type can include a `Stream` property. The client sends the file content as the request body while the other properties are sent as metadata.

### Server handler

```csharp
public class FileUpload : MediatR.IRequest
{
    public string Name { get; set; }
    public Stream Content { get; set; }
}

public class FileUploadHandler : MediatR.IRequestHandler<FileUpload>
{
    public async Task Handle(FileUpload request, CancellationToken cancellationToken)
    {
        var safeName = Path.GetFileName(request.Name);
        using var fileStream = File.Create(safeName);
        await request.Content.CopyToAsync(fileStream, cancellationToken);
    }
}
```

This handler strips directory components from the supplied name and writes only under a dedicated upload directory. In production, also enforce authentication, authorization, file-size limits, and any required content checks.

### JavaScript client

```js
import { WebMediatorClient } from 'web-mediator-client';


const client = new WebMediatorClient('https://localhost:7263/mediator');

let file = document.getElementById('my-input').files[0];

await client.send('FileUpload', { name: file.name, content: file });
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript/test.js)




## Example 7: Generic requests

Register the generic type definitions that may be used, then address a constructed type using its serialized name:

```csharp
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

Return an async stream of `SseItem<T>` values to send server-sent events to clients.

```csharp
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

Return an async stream of regular values when the client should consume a sequence rather than an SSE event stream.

```csharp
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

*Adding IStreamRequest handling to the endpoint*
```C#
app.MapMediator("mediator",
    // register possible request types
    cfg => cfg
        .RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly)
        .RegisterTypes([typeof(AsyncItemsStream)]),

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
[Example code...](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.MediatR/Program.cs)


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

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

