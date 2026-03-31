# WebMediator [![NuGet version](https://badge.fury.io/nu/WebMediator.svg?1)](http://badge.fury.io/nu/WebMediator)
A universal WebApi endpoint for any mediators.


## Features
* Suitable for any mediators
* In/out file streams support
* Generics request types support
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
GET /mediator/Ping?data={"Message":"TEST"}

or

POST /mediator/Ping
{"Message":"TEST"}
```

*Response*
```
{"Message":"TEST PONG"}
```

## Example 3: .NET client
*.NET CLI*
```
dotnet new console --name "WebMediatorClientExample"
cd WebMediatorClientExample
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

*Console output:*
```
TEST PONG
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

let response = await client.send('Ping', { Message: 'TEST' });

console.log(response.data);
```

*Console output:*
```
{"Message":"TEST PONG"}
```

[JS Project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript)


## Example 5: File upload
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

let response = await client.send('FileUpload', { Name: file.name, Content: file });
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/WebMediator.Client.JavaScript/test.js)




## Example 6: Generics requests
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
