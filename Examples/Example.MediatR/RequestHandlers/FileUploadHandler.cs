using MediatR;
namespace Example.MediatR.Handlers;

public class FileUploadHandler : IRequestHandler<FileUpload>
{
    public async Task Handle(FileUpload request, CancellationToken cancellationToken)
    {
        using var fileStream = File.Create(GetPath(request.Name));

        await request.Content.CopyToAsync(fileStream, cancellationToken);
    }

    internal static string GetPath(string filename)
    {
        return Path.Combine([AppContext.BaseDirectory, filename ?? throw new ArgumentNullException(nameof(filename))]);
    }
}

