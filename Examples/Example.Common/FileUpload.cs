using MediatR;
using System.IO;

namespace Example;

public class FileUpload : IRequest
{
    public string Name { get; set; }
    public Stream Content { get; set; }
}
