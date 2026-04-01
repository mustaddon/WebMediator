using MediatR;
using System.IO;

namespace Example;

public class FileDownload : IRequest<Stream>
{
    public string Name { get; set; }
}

public class FileDownloadWithInfo : IRequest<FileDownloadWithInfoResponce>
{
    public string Name { get; set; }
}

public class FileDownloadWithInfoResponce
{
    public string Name { get; set; }
    public long Size { get; set; }
    public Stream Content { get; set; }
}