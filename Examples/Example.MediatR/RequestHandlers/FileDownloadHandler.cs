using MediatR;
namespace Example.MediatR.Handlers;

public class FileDownloadHandler : IRequestHandler<FileDownload, Stream>
{
    public async Task<Stream> Handle(FileDownload request, CancellationToken cancellationToken)
    {
        return File.OpenRead(Path.GetFullPath(request.Path!));
    }
}


public class FileDownloadWithInfoHandler : IRequestHandler<FileDownloadWithInfo, FileDownloadWithInfoResponce>
{
    public async Task<FileDownloadWithInfoResponce> Handle(FileDownloadWithInfo request, CancellationToken cancellationToken)
    {
        return new()
        {
            Name = Path.GetFileName(request.Path!),
            Content = File.OpenRead(Path.GetFullPath(request.Path!)),
        };
    }
}