using MediatR;
using System.Reflection;
namespace Example.MediatR.Handlers;

public class FileUploadHandler : IRequestHandler<FileUpload, string>
{
    public async Task<string> Handle(FileUpload request, CancellationToken cancellationToken)
    {
        var filePath = Path.GetFullPath(Path.Combine([
            Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!,
            request.Name ?? throw new ArgumentNullException(nameof(request.Name))]));
        using var fileStream = File.Create(filePath);
        await request.Content.CopyToAsync(fileStream, cancellationToken);
        return filePath;
    }
}

