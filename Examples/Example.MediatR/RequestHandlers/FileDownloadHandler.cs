using MediatR;
namespace Example.MediatR.Handlers;

public class FileDownloadHandler : IRequestHandler<FileDownload, Stream>
{
    public async Task<Stream> Handle(FileDownload request, CancellationToken cancellationToken)
    {
        return File.OpenRead(FileUploadHandler.GetPath(request.Name));
    }
}


public class FileDownloadWithInfoHandler : IRequestHandler<FileDownloadWithInfo, FileDownloadWithInfoResponce>
{
    public async Task<FileDownloadWithInfoResponce> Handle(FileDownloadWithInfo request, CancellationToken cancellationToken)
    {
        var info = new FileInfo(FileUploadHandler.GetPath(request.Name));
        
        return new()
        {
            Name = info.Name,
            Size = info.Length,
            Content = info.OpenRead(),
        };
    }
}