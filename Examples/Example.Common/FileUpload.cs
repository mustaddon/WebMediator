using MediatR;
using System.IO;

namespace Example;

public class FileUpload : IRequest<string>
{
    public string Name { get; set; }
    public Stream Content { get; set; }
}
