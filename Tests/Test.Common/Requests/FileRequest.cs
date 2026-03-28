using MediatR;
using System.IO;

namespace Test.Requests
{
    public class FileRequest : IRequest<FileResponse>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class FileResponse
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public Stream? Content { get; set; }
        public FileMetadata? Metadata { get; set; }

        public bool InlineDisposition { get; set; }
    }
}
