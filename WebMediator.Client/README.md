# WebMediator.Client [![NuGet version](https://badge.fury.io/nu/WebMediator.Client.svg?1)](https://www.nuget.org/packages/WebMediator.Client)

A .NET client for sending requests to a [WebMediator](https://github.com/mustaddon/WebMediator) endpoint. It supports typed request/response calls, void requests, file and stream transfers, and server-sent event streams.

## Install

```bash
dotnet add package WebMediator.Client
```

## Send a request

Use `WebMediatorClient` with the URL of your mapped mediator endpoint. Request and response types are your application’s shared contracts; this example uses `Ping` and `Pong` from the sample project.

```csharp
using WebMediator.Client;

using var client = new WebMediatorClient("https://localhost:7263/mediator");
var response = await client.Send(new Ping { Message = "TEST" });

Console.WriteLine(response?.Message);

// Build a GET URL for a request without stream data.
var requestUrl = client.GetUrl(new Ping { Message = "TEST" });
```

Example output:

```text
TEST PONG
```

The client implements `IDisposable`; dispose it when it is no longer needed. Pass a cancellation token to `Send` to cancel a request:

```csharp
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var response = await client.Send(new Ping { Message = "TEST" }, cancellation.Token);
```

## Void requests

For a request that does not return a response body, call `Send` and await the operation:

```csharp
await client.Send(new VoidRequest { Message = "Process this request" });
```

## File and stream transfers

Requests and responses can carry streams. Dispose returned streams after use so the underlying HTTP response can be released:

```csharp
using var stream = await client.Send<Stream>(new FileDownload { Name = "example.txt" });
using var reader = new StreamReader(stream!);
Console.WriteLine(await reader.ReadToEndAsync());
```

For upload requests, put the file stream in a request property of type `Stream` and dispose the stream you opened. See the [complete client example](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.Client) for upload, download, and response metadata examples.

## Server-sent events

Use `EventStream` to consume typed SSE items. The request contract below is from the sample project:

```csharp
using Example;

using var cancellation = new CancellationTokenSource();

await foreach (var item in client.EventStream<string>(
    new ExampleAsyncEvents { Type = "updates" },
    cancellation.Token))
{
    Console.WriteLine($"{item.EventType}: {item.Data}");
}
```

## Configure the client

Pass `WebMediatorClientSettings` to customize JSON serialization, add default HTTP headers, provide an HTTP handler, or configure event-stream reconnection:

```csharp
var settings = new WebMediatorClientSettings
{
    HttpHeaders = new Dictionary<string, IEnumerable<string>>
    {
        ["X-Client-Name"] = ["sample-app"]
    }
};

using var client = new WebMediatorClient(
    "https://localhost:7263/mediator",
    settings);
```

## More examples

See the [Example.Client project](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.Client) for working examples of request/response calls, uploads, downloads, and streams. For JavaScript and Python clients, see their guides in the [repository](https://github.com/mustaddon/WebMediator).
