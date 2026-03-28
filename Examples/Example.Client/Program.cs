using Example;
using System.Text;
using WebMediator.Client;


// create client
using var mediator = new WebMediatorClient("https://localhost:7263/mediator");


// ping request
var pingResponse = await mediator.Send(new Ping { Message = "TEST" });
Console.WriteLine($"ping result: {pingResponse?.Message}");


// void request
await mediator.Send(new VoidRequest { Message = "TEST VOID" });


// file upload request
var uploadResponse = await mediator.Send(new FileUpload
{
    Content = new MemoryStream(Encoding.UTF8.GetBytes("text text text !!!")),
    Name = "example.txt",
});
Console.WriteLine($"upload result: {uploadResponse}");


// file download request
using var downloadResponse = await mediator.Send(new FileDownload { Path = @".\example.txt" });
using var reader = new StreamReader(downloadResponse!);
Console.WriteLine($"downloaded result: {await reader.ReadToEndAsync()}");


// download file with metadata
var downloadResponse2 = await mediator.Send(new FileDownloadWithInfo { Path = @".\example.txt" });
using var reader2 = new StreamReader(downloadResponse2!.Content!);
Console.WriteLine($"downloaded name: '{downloadResponse2.Name}', content: {await reader2.ReadToEndAsync()}");


Console.WriteLine("done");