# WebMediator Client [![NuGet version](https://badge.fury.io/nu/WebMediator.Client.svg)](http://badge.fury.io/nu/WebMediator.Client)
WebMediator API client


## Usage
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

Console.WriteLine(response?.Message);
```

*Console output:*
```
TEST PONG
```

[Example project...](https://github.com/mustaddon/WebMediator/tree/main/Examples/Example.Client)
