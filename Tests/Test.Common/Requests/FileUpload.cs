using MediatR;
using System.IO;

namespace Test.Requests
{
    public class FileUpload : IRequest<FileUploadResult>
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public Stream? Content { get; set; }
    }

    public class FileUpload<TMetadata> : IRequest<FileUploadResult<TMetadata>>
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public Stream? Content { get; set; }
        public TMetadata? Metadata { get; set; }
    }

    public class FileUploadResult
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
    }

    public class FileUploadResult<TMetadata> : FileUploadResult
    {
        public TMetadata? Metadata { get; set; }
    }
}
